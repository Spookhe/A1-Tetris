/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int Position { get; private set; }
    public int rotationIndex { get; private set; }

    public float stepDelay = 1f;
    public float moveDelay = 0.1f;
    public float lockDelay = 0.5f;

    private float stepTime;
    private float moveTime;
    private float lockTime;

    private void Update()
    {
        // Clear and set piece on board each frame
        board.Clear(this);

        lockTime += Time.deltaTime;

        HandleRotationInput();
        HandleMovementInput();
        HandleHardDrop();

        if (Time.time >= stepTime)
        {
            Step();
        }

        board.Set(this);
    }

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.board = board;
        this.data = data;
        this.Position = position;
        this.rotationIndex = 0;

        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null || cells.Length != data.cells.Length)
            cells = new Vector3Int[data.cells.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void HandleRotationInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Rotate(-1);
        else if (Input.GetKeyDown(KeyCode.E))
            Rotate(1);
    }

    private void HandleMovementInput()
    {
        if (Time.time < moveTime) return;

        // Soft drop
        if (Input.GetKey(KeyCode.S))
        {
            if (Move(Vector2Int.down))
                stepTime = Time.time + stepDelay;
        }

        // Left/right movement
        if (Input.GetKey(KeyCode.A))
            Move(Vector2Int.left);
        else if (Input.GetKey(KeyCode.D))
            Move(Vector2Int.right);
    }

    private void HandleHardDrop()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            while (Move(Vector2Int.down)) { }
            Lock();
        }
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;
        Move(Vector2Int.down);

        if (lockTime >= lockDelay)
            Lock();
    }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();

        // Only award time/score if this is the Fork piece
        if (data.tetromino == Tetromino.Fork)
        {
            TimeTrialManager timeTrial = FindObjectOfType<TimeTrialManager>();
            if (timeTrial != null)
            {
                timeTrial.OnForkPlaced();
            }
        }

        board.SpawnPiece();
    }


    private bool Move(Vector2Int translation)
    {
        Vector3Int newPos = Position;
        newPos.x += translation.x;
        newPos.y += translation.y;

        if (board.IsValidPosition(this, newPos))
        {
            Position = newPos;
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // Reset lock timer
            return true;
        }

        return false;
    }

    // Function to rotate tetronimo pieces

    private void Rotate(int direction)
    {
        int originalRotation = rotationIndex;
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];
            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    // Wrapper functions used for tetronimo wallkicks

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];
            if (Move(translation)) return true;
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;
        if (rotationDirection < 0) wallKickIndex--;
        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min) return max - (min - input) % (max - min);
        return min + (input - min) % (max - min);
    }
}
