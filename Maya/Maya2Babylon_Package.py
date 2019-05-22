import os
import zipfile
import shutil

versions = ['2017-2018', '2019']
projectFolderPrefix = './'
buildPathPrefix = 'bin/Release/'

packageFolderPrefix = './'
Maya2BabylonPackagePrefix = 'Maya2Babylon-'
Maya2BabylonVersion = '1.3.0'

with zipfile.ZipFile(packageFolderPrefix + Maya2BabylonPackagePrefix + Maya2BabylonVersion + '.zip', 'w' ) as outputZip:

    for version in versions:
        # get file paths
        buildPath = projectFolderPrefix + buildPathPrefix + version + '/'
        buildDlls = [ dll for dll in os.listdir(buildPath) if dll.endswith('.dll')]

        packagePath = version + '/'
        # copy bins to publish location
        for dll in buildDlls:
            packageDll = dll
            if 'Maya2Babylon' in dll:
                packageDll = 'Maya2Babylon.nll.dll'
            outputZip.write(buildPath + dll, packagePath + packageDll)