import os
import zipfile
import shutil

versions = ['2015', '2017', '2018', '2019']
projectFolderPrefix = './'
buildPathPrefix = '/bin/Release/'

packageFolderPrefix = '../'
Max2BabylonPackagePrefix = 'Max2Babylon-'
Max2BabylonVersion = '1.4.0'

with zipfile.ZipFile(packageFolderPrefix + Max2BabylonPackagePrefix + Max2BabylonVersion + '.zip', 'w' ) as outputZip:

    for version in versions:
        # get file paths
        buildPath = projectFolderPrefix + version + buildPathPrefix
        buildDlls = [ dll for dll in os.listdir(buildPath) if dll.endswith('.dll')]

        if version == '2015':
            packagePath = '2015 - 2016' + '/'
        else:
            packagePath = version + '/'

        # copy bins to publish location
        for dll in buildDlls:
            outputZip.write(buildPath + dll, packagePath + dll)

