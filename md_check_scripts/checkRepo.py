# -*- coding: UTF-8 -*-
import argparse, os
import mistune
from glob import glob
from pathlib import Path
from bs4 import BeautifulSoup

PURPOSE_STATEMENT_MIN = 300
FACT_RESULT_MIN = 1200
ANALOGS_MIN = 3
CRITERIAS_MIN = 3
ANALOG_REVIEW_MIN = 2000

def CheckRepo(repoPath):
    print('Checking for directory "' + repoPath + '"')
    directory = Path(repoPath)
    if directory.is_dir():
        print(' Directory'.ljust(26) +  '...\tExists!') # \u2713
        CheckGroupDirectory(repoPath + '\\3303')
        CheckGroupDirectory(repoPath + '\\3304')
        CheckGroupDirectory(repoPath + '\\3381')
    else:
        print(" Directory doesn't exist!")

def CheckGroupDirectory(groupFolderPath):
    print('Checking for directory "' + groupFolderPath + '"')
    directory = Path(groupFolderPath)
    if directory.is_dir():
        print(' Directory'.ljust(26) +  '...\tExists!') # \u2713
        for subfolder in os.listdir(groupFolderPath):
            CheckNamedDirectory(groupFolderPath+'\\'+subfolder)
    else:
        print(" Directory doesn't exist!")

def CheckNamedDirectory(nameFolderPath):
    print('Checking for directory "' + nameFolderPath + '"')
    directory = Path(nameFolderPath)

    if directory.is_dir():
        print(' Directory'.ljust(26) +  '...\tExists!') # \u2713
        CheckForPaperBase(nameFolderPath + '\\paper_base')
        CheckForFactResult(nameFolderPath)
        CheckPurposeStatementFiles(nameFolderPath)
        CheckForAnalogs(nameFolderPath)     
    else:
        print(" Directory doesn't exist!")

def CheckForPaperBase(paperBaseFolderPath):
    #.pdf file(s)
    print("Checking \\paper_base directory for .pdf files")
    if os.path.isdir(paperBaseFolderPath):
        print(' Directory'.ljust(26) +  '...\tExists!')
        pdfFiles = glob(os.path.join(paperBaseFolderPath,"*.{}".format('pdf')))
        if len(pdfFiles) != 0:
            print(' .pdf files found:')
            for pdfFile in pdfFiles:
                print('  ' + os.path.basename(pdfFile))
        else:
            print('  No .pdf files!')
    else:
        print(" Directory doesn't exist!")

def CheckForFactResult(factResFolderPath):
    print("Checking for fact_result.md")
    if os.path.isfile(factResFolderPath+'\\fact_result.md'):
        print(' fact_result.md'.ljust(26) +  '...\tExists!')
        factSymbolsNum = CountTextSymbols(factResFolderPath+"\\fact_result.md")
        if factSymbolsNum >= FACT_RESULT_MIN:
            print("  Number of symbols in fact_result.md".ljust(50) + '...\tGood.')
        else:
            print("  Number of symbols in fact_result.md".ljust(50) + '...\tNot enough. Min = ' + \
                str(FACT_RESULT_MIN))
    else:
        print(' fact_result.md'.ljust(26) +  '...\tdoes not exist!')

def CheckPurposeStatementFiles(nameFolderPath):
    print('Checking for purpose-statement files...')
    if (
        CheckForFile(nameFolderPath,"problem.md") &
        CheckForFile(nameFolderPath,"research_object.md") &
        CheckForFile(nameFolderPath,"research_subject.md") &
        CheckForFile(nameFolderPath,"goal.md") &
        CheckForFile(nameFolderPath,"tasks.md") &
        CheckForFile(nameFolderPath,"relevance.md") ):
        print(' All required files exist!\n  Counting symbols...')            
        purposeSymbNum = CountPurposeTextSymbols(nameFolderPath)
        if purposeSymbNum >= PURPOSE_STATEMENT_MIN:
            print("  Number of symbols in purpose statement's".ljust(50) + '...\tGood.')
        else:
            print("  Number of symbols in purpose statement's".ljust(50) + '...\tNot enough. Min = ' + \
                str(PURPOSE_STATEMENT_MIN))
    else:
        print(' Not all of required files exist!')

def CountPurposeTextSymbols(folderPath):
    allSymbolsNum = CountTextSymbols(folderPath + "\\problem.md") + \
            CountTextSymbols(folderPath+"\\research_object.md") + \
            CountTextSymbols(folderPath+"\\research_subject.md") + \
            CountTextSymbols(folderPath+"\\goal.md") + \
            CountTextSymbols(folderPath+"\\tasks.md") + \
            CountTextSymbols(folderPath+"\\relevance.md")
    if allSymbolsNum == 0:
        print('  All files are empty!')
    else:
        print('  All files'.ljust(26) + '...\t' + str(allSymbolsNum) + ' symbols')
    return allSymbolsNum

def CheckForAnalogs(analogsFolderPath):
    print("Checking for analogs.md")
    if os.path.isfile(analogsFolderPath+'\\analogs.md'):
        print(' analogs.md'.ljust(26) +  '...\tExists!')
        factSymbolsNum = CountTextSymbols(analogsFolderPath+"\\analogs.md")
        if factSymbolsNum >= ANALOG_REVIEW_MIN:
            print("  Number of symbols in analogs.md".ljust(50) + '...\tGood.')
        else:
            print("  Number of symbols in analogs.md".ljust(50) + '...\tNot enough. Min = ' + \
                str(ANALOG_REVIEW_MIN))
        # .md parsing:
        soup = ParseMd(analogsFolderPath+'\\analogs.md')
        titles = soup.find_all('h2')

        analogsH = 'none'
        criteriasH = 'none'
        sourcesH = 'none'

        for title in titles:
            if title.text.find("Сравнение аналогов") != -1:
                analogsH = title
            if title.text.find("Критерии сравнения аналогов") != -1:
                criteriasH = title
            if title.text.find("Источники") != -1:
                sourcesH = title
        
        if analogsH == 'none':
            print("  There is no Analogs title!")
        else: # analogs check
            print("  Analogs title!".ljust(26) +  '...\tExists!')
            #find all h3 siblings
            analogChildren = []
            nextNode = analogsH
            while True:
                nextNode = nextNode.find_next_sibling()
                tag_name = ''
                try:
                    tag_name = nextNode.name
                except AttributeError:
                    tag_name = ""
                if tag_name == "h3":
                    analogChildren.append(nextNode)
                if tag_name == "h2":
                    break

            if len(analogChildren) >= ANALOGS_MIN:
                print("   Number of analogs".ljust(25) + '...\tGood.')
            else:
                print("   Number of analogs".ljust(25) + '...\tNot enough. Min = ' + \
                str(ANALOGS_MIN))          
        
        if criteriasH == 'none':
            print("  There is no Criterias title!")
        else: #criterias check
            print("  Criterias title!".ljust(26) +  '...\tExists!')
            
            #find all h3 siblings
            criteriaChildren = []
            nextNode = criteriasH
            while True:
                nextNode = nextNode.find_next_sibling()
                tag_name = ''
                try:
                    tag_name = nextNode.name
                except AttributeError:
                    tag_name = ""
                if tag_name == "h3":
                    criteriaChildren.append(nextNode)
                if tag_name == "h2":
                    break
                        
            if len(criteriaChildren) >= CRITERIAS_MIN:
                print("   Number of analogs".ljust(25) + '...\tGood.')
            else:
                print("   Number of analogs".ljust(25) + '...\tNot enough. Min = ' + \
                str(CRITERIAS_MIN))

        if sourcesH == 'none':
            print("  There is no Sources title!")
        else: #sources check
            print("  Sources title!".ljust(26) +  '...\tExists!')
            sourcesOl = sourcesH.find_next('ol')
            sourcesList = sourcesOl.findAll('li')
            if len(sourcesList) >= 1:
                print("   Number of sources".ljust(25) + '...\tAt least 1! Good!')
            else:
                print("   Number of sources".ljust(25) + '...\tNot enough. Min = 1')
    else:
        print(' analogs.md'.ljust(26) +  '...\tdoes not exist!')

def CheckForFile(dirpath, filename):
    file = Path(dirpath+'/' + filename)
    
    if file.is_file():
        print('     ' + filename.ljust(25) + '...\tExists!')
        return True
    else:
        print('     ' + filename.ljust(25) + "...\tDoesn't exist!")
        return False

def ParseMd(file):
    f = open(file, 'r')
    mdText = f.read()
    htmlText = mistune.markdown(mdText)
    soup = BeautifulSoup(htmlText, 'html.parser')
    return soup

def CountTextSymbols(filename):
    soup = ParseMd(filename)
    pars = soup.find_all('p')
    allText = ""
    for t in pars:
        allText += t.text
    NONCOUNT_LETTERS = " ,.:;!?"
    symbolsNum = len([letter for letter in allText if letter not in NONCOUNT_LETTERS])
    if symbolsNum == 0:
        print('    ' + os.path.basename(filename).ljust(25) + '...\tEmpty!')
    else:
        print('    ' + os.path.basename(filename).ljust(25) + '...\t' + str(symbolsNum) + ' symbols')
    return symbolsNum

parser = argparse.ArgumentParser()
parser.add_argument('path', help='path to directory with .md files')
args = parser.parse_args()
repo_path = args.path

CheckRepo(repo_path)