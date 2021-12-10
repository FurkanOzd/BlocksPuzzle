# BlocksPuzzle


 Proceduaral Level Generation Algorithm

- Choose random difficulty from difficultyList(Easy,Medium,Hard)
- Choose random boardSize and pieceCount from chosen difficulty
- Create grid (boardSize x boardSize)
- Create 4 triangles from every square
- Create points from triangles corners
- Create shapes equal to pieceCount
- Pick random points from grid, and set these points to shape centers.
- Select nearest triangles to points, and add to shapes
- Combine shape triangles meshes into one mesh and set to shape
