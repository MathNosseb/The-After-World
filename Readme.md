# The After World

![Gif du jeu](Docs/trailer.gif)
(les artefacts sont uniquement visible sur la video)

## Fonctionnalités
The After World est un jeu développé sur Unity, c’est un jeu qui m’a permis
d’apprendre comment fonctionnent les mouvements planétaires, la physique et les shaders. 
Il n’y a pas vraiment de but : on peut prendre un vaisseau pour se déplacer dans l’univers. 
L’inspiration vient du jeu **Outer Wilds**, avec des planètes réduites et des graphismes low poly. 

## Physique
La physique du jeu est basée sur un schéma semi-implicite d’Euler. Je calcule l’attraction avec F = G * (m1 * m2) / r² <br>
J’applique ensuite la gravité avec AddForce, la physique est donc calculée réellement, les planètes orbitent de manière physiquement
réaliste.

## Rendu de planètes
Les planètes sont rendues procéduralement en utilisant [FastNoiseLite](https://github.com/Auburn/FastNoiseLite). J’utilise en réalité un fork qui le 
rend compatible avec **Burst**, ce qui permet de rendre les planètes bien plus rapidement. Cela m’a permis d’apprendre comment fonctionnent les jobs et 
l’optimisation. À l’origine, j’avais mis un système qui permettait, en utilisant le GPU, de rendre énormément d’herbe, mais le rendu ne me convenait pas.  
Il est encore possible de l’activer. <br>
Les rendus des atmosphères sont faits avec les tutos de [Sebastian Lague](https://www.youtube.com/c/SebastianLague).  
L’atmosphère utilise le Rayleigh scattering, permettant de rendre des atmosphères qui réagissent avec la lumière.

## Images
![Gif du jeu](Docs/3.PNG)
![Gif du jeu](Docs/5.PNG)
![Gif du jeu](Docs/7.PNG)
![Gif du jeu](Docs/11.PNG)
![Gif du jeu](Docs/12.PNG)