# Blocks Puzzle Game


 Levels are generated procedurally.

 Level Generation Process:
 - Choose random boardSize and piece count from randomly chosen difficulty
 - Create grid (boardSize x boardSize)
 - Create as many shapes as the selected number of pieces. 
 - Pick random points from grid, and set these points to shape centers
 - Create 4 triangles from every grid square
 - Select nearest triangles to shape centers, and add to shapes
 - Combine shape triangles meshes into one mesh and set it to shape
