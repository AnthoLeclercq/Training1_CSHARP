# Training1_CSHARP

Projet C# regroupant plusieurs exercices de programmation et SQL. Chaque exercice est accessible depuis un menu principal interactif.

---

## Description

Ce projet contient quatre exercices :

### 1. Exercice 1 - SQL en mémoire
- Utilisation de SQLite en mémoire avec les tables `Clients` et `Historique`.
- Requêtes exécutées :
  - Nombre de colis par client par année.
  - Moyenne des températures et précipitations par année, mois et jour (conditions : Temp ≥ 5 et Précipitations > 0).
- Les résultats sont affichés directement dans la console.

### 2. Exercice 2 - Premier trou dans une suite
- Trouve le premier entier positif manquant dans une liste de nombres fournie par l’utilisateur.
- La saisie se fait via une liste de nombres séparés par des virgules.
- Exclut les valeurs négatives et retourne le premier entier strictement supérieur à 0 qui n’est pas présent.

### 3. Exercice 3 - Fusion de plages horaires
- Permet de saisir des plages horaires par jour au format `Jour HH:mm-HH:mm`.
- Les plages se chevauchant sont automatiquement fusionnées par jour.
- Validation robuste :
  - Vérifie que le jour existe (Lundi à Dimanche).
  - Vérifie que les heures sont au format valide `HH:mm`.
  - Ignore les plages invalides (début après fin).
- Affichage clair en 24h avec numéro de plage par jour.

### 4. Exercice 4 - Code César (+3)
- Crypte et décrypte un texte avec un décalage de 3 positions dans l’alphabet.
- Fonctionne pour majuscules, minuscules et laisse les caractères non alphabétiques inchangés.
- Affiche le texte original, crypté et décrypté dans la console.