# BEFORE USING THIS SCRIPT, PLEASE MAKE SURE PYTHON AND THE PACKAGES LISTED IN THE README.MD ARE INSTALLED.

# ---------- IMPORTS ----------
import os
import zipfile
import shutil
import json     # Changelog
import requests  # Changelog
import datetime  # Changelog


versions = ['2017-2018', '2019']
projectFolderPrefix = './'
buildPathPrefix = 'bin/Release/'

packageFolderPrefix = './'
Maya2BabylonPackagePrefix = 'Maya2Babylon-'

currentVersionTime = datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S")
lastVersionTime = ''


# ---------- EXPORTER VERSION ----------
# Retrieved for the "Exporter/BabylonExporter.cs" file

Maya2BabylonVersion = ''
with open('Exporter/BabylonExporter.cs', 'r') as file_BabylonExporterCS:
    lineIndex = 0
    for line in file_BabylonExporterCS:
        lineIndex += 1
        if line.find('exporterVersion = "') != -1:
            firstOcc = line.find('"')
            lastOcc = line.find('"', firstOcc + 1)
            Maya2BabylonVersion = line[firstOcc + 1: lastOcc]
            break
file_BabylonExporterCS.close()
# Be sure that the version has been found. Otherwise, raise a Value Error
if (Maya2BabylonVersion == ''):
    raise ValueError(
        "Impossible to fine line with 'string exporterVersion = \"' in the BabylonExporter.cs file")


# ---------- UPDATING THE CHANGELOG.MD ----------

# Get informations from the repo
print("Please, enter your Github Access Token.")
print("See the readme.md for more informations at ## Build package")
print("Leave blank and press enter if you don't want to fill the Changelog")
accessToken = str(input("Access Token : "))

def updateChangelog () :

    queryString = """
    {
    repository(owner: "BabylonJS", name: "Exporters") {
        pullRequests(first:25, states:MERGED, labels:"maya", orderBy:{field:UPDATED_AT, direction:DESC}) {
        nodes {
            updatedAt,
            closedAt,
            number,
            title,
            labels(first:10) {
            edges {
                node {
                name
                }
            }
            }
        }
        }
    }
    }
    """
    query = {'query': queryString.replace('\n', ' ')}
    headers = {'Authorization': 'token ' + accessToken,
            'Content-Type': 'application/json'}

    response = requests.post(
        'https://api.github.com/graphql', headers=headers, json=query)

    if ('message' in json.loads(response.text)) :
        print(" ==> /!\\ Error when trying to reach the Github API : {0} \n".format(json.loads(response.text)['message']))
        return

    data = json.loads(response.text)['data']['repository']['pullRequests']['nodes']

    # Get last version date
    with open('{0}CHANGELOG.md'.format(packageFolderPrefix), 'r') as file_Changelog:
        lineIndex = 0
        for line in reversed(file_Changelog.readlines()):
            lineIndex += 1
            if line.find('### (2') != -1:
                firstOcc = line.find('(')
                lastOcc = line.find(')', firstOcc + 1)
                lastVersionTime = line[firstOcc + 1: lastOcc]
                break
    file_Changelog.close()

    # Then write PR titles in the Changelog
    list_bug = []
    list_enhancement = []
    with open('{0}CHANGELOG.md'.format(packageFolderPrefix), 'a') as file_Changelog:
        file_Changelog.write("\n## v" + Maya2BabylonVersion)
        file_Changelog.write("\n### ({0})".format(currentVersionTime))

        for row in data:
            isYounger = (lastVersionTime < row['closedAt'].replace(
                'T', ' ').replace('Z', ''))
            if isYounger:
                for label in row['labels']['edges']:
                    if label['node']['name'] == 'bug':
                        list_bug.append(row)
                    elif label['node']['name'] == 'enhancement':
                        list_enhancement.append(row)

        if len(list_enhancement) > 0:
            file_Changelog.write("\n**Implemented Changes**")
            for row in list_enhancement:
                file_Changelog.write(
                    "\n- {0} (https://github.com/BabylonJS/Exporters/pull/{1})".format(row['title'], row['number']))
                file_Changelog.write('\n')

        if len(list_bug) > 0:
            file_Changelog.write("\n**Fixed Bugs**")
            for row in list_bug:
                file_Changelog.write(
                    "\n- {0} (https://github.com/BabylonJS/Exporters/pull/{1})".format(row['title'], row['number']))
                file_Changelog.write('\n')

    file_Changelog.close()

if (accessToken != ''): updateChangelog()
else: print(" ==> /!\\ Changelog will not be completed \n")


# ---------- BUILD THE ZIP FILE FROM DLL ----------

with zipfile.ZipFile(packageFolderPrefix + Maya2BabylonPackagePrefix + Maya2BabylonVersion + '.zip', 'w') as outputZip:

    for version in versions:
        # get file paths
        buildPath = projectFolderPrefix + buildPathPrefix + version + '/'
        buildDlls = [dll for dll in os.listdir(
            buildPath) if dll.endswith('.dll')]

        packagePath = version + '/'
        # copy bins to publish location
        for dll in buildDlls:
            packageDll = dll
            if 'Maya2Babylon' in dll:
                packageDll = 'Maya2Babylon.nll.dll'
            outputZip.write(buildPath + dll, packagePath + packageDll)
