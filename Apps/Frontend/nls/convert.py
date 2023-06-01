# Convert from a CSV format to a YAML format

import yaml
from bs4 import BeautifulSoup
import cgi

def HTMLEntitiesToUnicode(html):
    return BeautifulSoup(html).text

title_nls = {
    "EN": "BC&nbsp;Demographic&nbsp;Survey",
    "ZZ": "&#21329;&#35799;&#20154;&#21475;&#35843;&#26597",
    "FA": "&#1606;&#1592;&#1585;&#1587;&#1606;&#1580;&#1740;&nbsp;&#1580;&#1605;&#1593;&#1740;&#1578;&#8204;&#1588;&#1606;&#1575;&#1582;&#1578;&#1740;&nbsp;&#1576;&#1585;&#1740;&#1578;&#1740;&#1588; &#1705;&#1604;&#1605;&#1576;&#1740;&#1575",
    "EN": "BC&nbsp;Demographic&nbsp;Survey",
    "TL": "BC&nbsp;Demographic&nbsp;Survey",
    "ES": "Encuesta demogr&#225;fica de BC",
    "KO": "&#48708;&#50472;&#51452;&nbsp;&#51064;&#44396;&#53685;&#44228;&nbsp;&#49444;&#47928;&#51312;&#49324",
    "PA": "&#2604;&#2624; &#2616;&#2624; &#2588;&#2600;&#2616;&#2672;&#2582;&#2623;&#2566; &#2616;&#2608;&#2613;&#2631;&#2582;&#2595",
    "VI": "Kh&#7843;o s&#225;t Nh&#226;n kh&#7849;u h&#7885;c&nbsp; BC",
    "FR": "Enqu&#234;te d&#233;mographique de la Colombie-Britannique",
    "HI": "&#2348;&#2368;&#2360;&#2368;&nbsp;&#2332;&#2344;&#2360;&#2366;&#2306;&#2326;&#2381;&#2351;&#2367;&#2325;&#2368;&#2351;&nbsp;&#2360;&#2352;&#2381;&#2357;&#2375;&#2325;&#2381;&#2359;&#2339",
    "AR": "BC&nbsp;Demographic&nbsp;Survey",
    "JA": "BC&#24030;&nbsp;&#20154;&#21475;&#32113;&#35336;&nbsp;&#35519;&#26619",
    "UR": "BC&nbsp;&#1672;&#1740;&#1605;&#1608;&#1711;&#1585;&#1575;&#1601;&#1705;&nbsp;&#1587;&#1585;&#1608;&#1746",
    "ZH": "&#21329;&#35433;&#30465;&#20154;&#21475;&#32113;&#35336;&#35519;&#26597",
    "PT": "Inqu&#233;rito Demogr&#225;fico de BC"
}

defaults_nls = {
    "incidentTitle": "The BC Demographic Survey is currently unavailable.",
    "incidentText": "We are working to fix this as quickly as possible."
}

locales = {
    "AR": "Arabic",
    "EN": "English",
    "ES": "Spanish",
    "FA": "Persian",
    "FR": "French",
    "HI": "Hindi",
    "JA": "Japanese",
    "KO": "Korean",
    "PA": "Filipino",
    "PT": "Portugese",
    "TL": "Tagalog",
    "UR": "Urdu",
    "VI": "Vietnamese",
    "ZH": "Chinese (Traditional)",
    "ZZ": "Chinese (Simplified)",
}

labels = [
     "appName",
     "title",
     "errorText",
     "incidentTitle",
     "incidentText",
     "componentFallbackText",
     "statusTitle",
     "statusPositionText",
     "errorTitle",
     "errorRetryText",
     "redirectText"
]



import csv

translations = []

def find_translation (locale, label):
    for trans in translations:
        if trans[0] == locale and trans[1] == label:
            return trans[2].strip()
    return None

with open('messages 2023-05-29.csv', newline='') as csvfile:
    spamreader = csv.reader(csvfile, delimiter=',', quotechar='"')
    for row in spamreader:
        if row[5] != "":
            parts = row[6].split(":")
            translations.append([row[5], parts[0], parts[1]])
            print("%5s : %20s : %s" % (row[5], parts[0], parts[1]) )

nls = {}

for locale in locales:
    print(locale)

    out_locale = locale.lower()
    if locale not in nls:
        nls[out_locale] = {}

    for label in labels:
        en = find_translation ('en', label)
        if label == 'title':
            en = title_nls[locale]
        if label == 'incidentTitle' or label == 'incidentText':
            en = defaults_nls[label]
        nls[out_locale][label] = HTMLEntitiesToUnicode(find_translation(locale, label) or en)

out_doc = {}
out_doc["locales"] = nls

with open('config_nls.yaml', 'w') as file:
    yaml.dump(out_doc, file)
