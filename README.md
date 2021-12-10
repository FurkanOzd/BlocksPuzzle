# BlocksPuzzle


 Proceduaral Level Generation Algorithm

- Choose random difficulty from difficultyList(Easy,Medium,Hard)
- Choose random boardSize and pieceCount from chosen difficulty
- Create square grid equals to (boardSize x boardSize)
- Create 4 triangles from every square
- Create points from triangles corners
- Pick randomPoints from grid, equals to pieceCount
- Create shapes equals to pieceCount
- Select nearest triangles to points, and add to shapes
- Combine triangles meshes into one mesh and set to shape
