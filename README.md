# Introduction

## Attestation Generator

Attestation Generator est un générateur d'attestations dérogatoires.<br />
Il est basé sur [l'outil](https://media.interieur.gouv.fr/deplacement-covid-19/) du ministère de l'intérieur.<br />
On y retrouve les mêmes fonctionnalités avec en plus un générateur de signature et une sauvegarde des champs textuels.<br />
Cet outil est un travail d'étudiant principalement pour apprendre le Framework .NET ainsi que WPF

## Installation

2 choix possibles : l'installer sur votre pc ou bien la version standalone qui n'a pas besoin d'être installée

## Utilisation
Lancez AttestationGenerator.exe

## Documentation

![](.gitbook/assets/attestationGenerator_main.PNG)

|Nom |Format | 
|:--- |:---- |
|Prénom| Charactère ASCII |
|Nom| Charactère ASCII |
|Date de naissance| JJ/MM/AAAA |
|Lieu de naissance| Charactère ASCII |
|Adresse| Charactère ASCII |
|Ville| Charactère ASCII |
|Code postal| 00000 |
|Date de sortie| JJ/MM/AAAA |
|Heure de sortie| HH:MM |

La signature s'effectue avec la souris <br/>
Vous pouvez cocher autant de cases que souhaité (même 0) <br />

Pour sauvegarder les champs il faut les avoir préremplis. L'heure de sortie s'actualise au moment où vous chargez un fichier.<br />

L'attestation générée sera dans le dossier où se trouve le .exe<br />
Si le gouvernement modifie le pdf il suffit de remplacer **certificate.pdf** qui se trouve dans le dossier d'installation par celui actualisé.<br />
Si le placement ne convient pas j'effectuerai une mise à jour du programme en conséquence.
