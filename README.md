# SignApk

A APK Sign program in C#, to sign the APK using a certificate without the need to recompile it

# Regarding the original code

According to the original project (the Sample project instructions and code) would require:

- An unsigned APK (or ZIP) file containing all the APK files, "classes.dex", assets, resources and manifest.
- A Pk12 key (that could be generated using any machanism, such as OpenSSL)

Verifing the code, the main windows would have a list of files (that were supposed to be in the APK), but would only pre-list the "classes.dex" file.

So the code to make the APK sign execution would be:

    string apk = "C:\\Users\\User\\Desktop\\APK\\original.apk";
    string extractedDir = "C:\\Users\\User\\Desktop\\APK\\extracted\\";
    //it would be from a list from a config.txt file, but as default has only that
    List<string> files = new List<string>(new string[] { "classes.dex" });
    //would then extract the files to a folder
    int unzipFileCount = ApkTool.Unzip(apk, files, extractedDir);
    //then incorporate it back, signing it
    string key = "C:\\Users\\User\\Desktop\\APK\\key.p12";
    string outputApk = "C:\\Users\\User\\Desktop\\APK\\signed.apk";
    int zipFileCount = ApkTool.ZipAndSign(apk, extractedDir, files, outputApk, key);
