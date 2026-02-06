/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    // Count pieces spawned, used to force Fork every 5th piece
    private int piecesSpawned = 0;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        TetrominoData data;

        // Spawns Fork every 5th piece
        piecesSpawned++;
        if (piecesSpawned % 5 == 0)
        {
            // Find Fork in tetromino array
            data = System.Array.Find(tetrominoes, t => t.tetromino == Tetromino.Fork);
            if (data.tetromino != Tetromino.Fork)
            {
                int random = Random.Range(0, tetrominoes.Length);
                data = tetrominoes[random];
            }
        }
        else
        {
            int random = Random.Range(0, tetrominoes.Length);
            data = tetrominoes[random];
        }

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles(); // Clears tiles from grid when time is up / Pieces fill the board
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.Position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.Position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    // Checks if tetronimo piece has valid positioning on the board

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition) || tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        // Start from bottom to top
        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++; // Only advance if current row is not full
            }
        }

        // Notify TimeTrialManager after all lines are cleared
        if (linesCleared > 0)
        {
            TimeTrialManager manager = FindObjectOfType<TimeTrialManager>();
            if (manager != null)
                manager.OnLinesCleared(linesCleared);
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!tilemap.HasTile(position)) return false;
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            tilemap.SetTile(new Vector3Int(col, row, 0), null);
        }

        // Shift rows above down
        for (int r = row + 1; r < bounds.yMax; r++)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int abovePos = new Vector3Int(col, r, 0);
                TileBase tile = tilemap.GetTile(abovePos);
                tilemap.SetTile(new Vector3Int(col, r - 1, 0), tile);
            }
        }
    }
}
