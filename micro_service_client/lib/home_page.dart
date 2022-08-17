import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:http/http.dart' as http;

class HomePage extends StatefulWidget {
  const HomePage({Key? key}) : super(key: key);

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  bool _isLoading = false;
  Map<String, dynamic> tmpMap = {};

  Future<void> _launchUrl(String link) async {
    final Uri url = Uri.parse(link);
    if (!await launchUrl(url, mode: LaunchMode.externalApplication)) {
      throw 'Could not launch $url';
    }
  }

  Future<void> _generateReport() async {
    setState(() {
      _isLoading = true;
    });

    var url = Uri.https('localhost:5001', 'api/report/generate');
    var response = await http.post(url);

    if (kDebugMode) {
      print('Response status: ${response.statusCode}');
    }

    setState(() {
      _isLoading = false;
    });
  }

  Stream<Map<String, dynamic>?> _getReport() async* {
    yield* Stream.periodic(const Duration(seconds: 3), (_) {
      var url = Uri.https('localhost:5001', 'api/report/get');

      try {
        return http.read(url);
      } catch (e) {
        if (kDebugMode) {
          print(e);
        }
      }
    }).asyncMap((event) async {
      try {
        final response = await event;

        if (response == null || response.isEmpty) {
          return tmpMap;
        }

        tmpMap = json.decode(response);
        return tmpMap;
      } catch (e) {
        if (kDebugMode) {
          print(e);
        }
      }

      return null;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Report'),
      ),
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            ElevatedButton(
              onPressed: _isLoading ? null : _generateReport,
              child: const Text('Generate report'),
            ),
            StreamBuilder<Map<String, dynamic>?>(
              stream: _getReport(),
              builder: (context, snapshot) {
                if (snapshot.hasData || tmpMap.isNotEmpty) {
                  if (snapshot.data == null) {
                    return ElevatedButton(
                      onPressed: () => _launchUrl(tmpMap['reportUrl']),
                      child: const Text('Download'),
                    );
                  }

                  return ElevatedButton(
                    onPressed: () => _launchUrl(snapshot.data!['reportUrl']),
                    child: const Text('Download'),
                  );
                }

                return const SizedBox.shrink();
              },
            ),
          ],
        ),
      ),
    );
  }
}
