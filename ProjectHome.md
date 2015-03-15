To be used with the Valve font rendering algorithm documented at http://www.valvesoftware.com/publications/2007/SIGGRAPH2007_AlphaTestedMagnification.pdf

Example output:

![http://i.imgur.com/zyRXC.png](http://i.imgur.com/zyRXC.png)

When passed though a renderer:

2x scale + anti-aliassing:
![http://i.imgur.com/4IZ6V.png](http://i.imgur.com/4IZ6V.png)

4x scale + alpha-testing
![http://i.imgur.com/e73vT.png](http://i.imgur.com/e73vT.png)


Notice, the artifact in the top-left corner is due to something else being rendered on top of the font.