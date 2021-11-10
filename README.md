# SignApk

A APK Sign program in C#, to sign the APK using a certificate without the need to recompile it

# Regarding the original code

According to the original project (the Sample project instructions and code) would require:

- An unsigned APK (or ZIP) file containing all the APK files, "classes.dex", assets, resources and manifest.
- A Pk12 key (that could be generated using any machanism, such as OpenSSL)

Verifing the code, the main windows would have a list of files (that were supposed to be in the APK), but would only pre-list the "classes.dex" file.

So the code to make the APK sign execution would be:

``` C#
    string apk = "C:\\User\\Desktop\\APK\\original.apk";
    string extractedDir = "C:\\User\\Desktop\\APK\\extracted\\";
    string key = "C:\\User\\Desktop\\APK\\cert.pfx";
    string password = "{password}";
    string outputApk = "C:\\User\\Desktop\\APK\\signed.apk";
    //it would be from a list from a config.txt file, but as default has only that
    List<string> files = new List<string>(new string[] { "classes.dex" });
    //would then extract the files to a folder
    int unzipFileCount = ApkTool.Unzip(apk, files, extractedDir);
    //then incorporate it back, signing it
    int zipFileCount = ApkTool.ZipAndSign(apk, extractedDir, files, outputApk, key, password: password);
    return ((zipFileCount == unzipFileCount) ? 0 : 1);
```

Don't know yet if this is the proper approach, since was a 5 year old code, and i don't know if "classes.dex" is all that it need, or should list all files from the apk in there (test for that is pending), but so faz the SharpZipLib required is placed on the "Namespace.ZIP.cs" file (it's an old version of it that i had modified) and the SignApk code is set on the "Namespace.APK.cs" file.

#State of Development

I might use the java source code for the ApkSign in "java/com/android/apksig" at googlesource.com ( https://android.googlesource.com/platform/tools/apksig/+/cb5e16ea45459c2cebfc532b45a0a185c124c34a/src/main/java/com/android/apksig?autodive=0%2F) since it's more complete - a java to c# port might do the trick
