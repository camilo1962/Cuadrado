using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Squared
{
    [AddComponentMenu("Squared/Board")]
    [DisallowMultipleComponent]
    public class Board : MonoBehaviour
    {
        #region Static Fields
        public static int LevelSOIndex = 0;
        public static Board instance;
        #endregion

        #region Campos del inspector
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private SceneTransition _sceneTransition = null;
        [SerializeField] private Vector2Int _boardSize = new Vector2Int(4, 4);
        [SerializeField] private TileSO[] _tileSOs = { };
        [SerializeField] private float _slideCooldown = 1;
        [SerializeField] private float _retryCooldown = 1;
        [SerializeField] public LevelSO[] _levelSOs = { };
        [SerializeField] public int numeroNivel;

        [SerializeField] public float timer;
        [SerializeField] private TMP_Text timerText;

        private float tiempoEmpleado;
        private int minutos, segundos, cents;
        #endregion

        #region Runtime Fields
        private Vector3 _worldZeroPosition = new Vector3(-1.5f, -1.5f);
        private List<Tile> _tiles = new List<Tile>();
        private List<Tile> _standardTiles = new List<Tile>();
        private Dictionary<Vector2Int, Tile> _tilesByPosition = new Dictionary<Vector2Int, Tile>();
        private Dictionary<TileSO, Stack<Tile>> _inactiveTiles = new Dictionary<TileSO, Stack<Tile>>();
        private float _slideCooldownTimer = 0;
        private float _retryCooldownTimer = 0;
        private bool _isRetrying = false;
        #endregion

        #region Properties
        public Vector2Int BoardSize
        {
            get => _boardSize;
            set
            {
                _boardSize = value;
                _renderer.size = _boardSize;
                _worldZeroPosition = new Vector3(-_boardSize.x / 2 + 0.5f, -_boardSize.y / 2 + 0.5f);
            }
        }
        #endregion
        public void Awake()
        {
            instance = this;
        }
        #region Unity Methods
        private void Start()
        {
           
            ReadTileSOs();
            StartLevel();
        }

        private void Update()
        {
            timer += Time.deltaTime;


            minutos = (int)(timer / 60f);
            segundos = (int)(timer - minutos * 60f);
            cents = (int)((timer - (int)timer) * 100);

            timerText.text = string.Format("{0}:{1}:{2}", minutos, segundos, cents);

            _slideCooldownTimer = Mathf.Clamp(_slideCooldownTimer - Time.deltaTime, 0, _slideCooldown);

            if (_isRetrying)
            {
                _retryCooldownTimer = Mathf.Clamp(_retryCooldownTimer - Time.deltaTime, 0, _retryCooldown);

                if (Mathf.Approximately(_retryCooldownTimer, 0))
                {
                    _isRetrying = false;
                    StartLevel();
                }
            }
        }
        #endregion

        #region Private Methods
        private void ReadTileSOs()
        {
            foreach (var tileSO in _tileSOs)
            {
                _inactiveTiles.Add(tileSO, new Stack<Tile>());
            }
        }

        private void PlaceInitialTiles(string tilemap)
        {
            string[] rows = tilemap.Split('\n');
            int y = _boardSize.y - 1;

            foreach (var row in rows)
            {
                for (int x = row.Length - 1; x >= 0; x--)
                {
                    string cell = $"{row[x]}";
                    if (cell == " ") continue;
                    PlaceTile(_tileSOs[int.Parse(cell)], new Vector2Int(x, y));
                }

                y--;
            }
        }

        public void StartLevel()
        {
            PlaceInitialTiles(_levelSOs[LevelSOIndex].Tilemap);
           
        }

        private Vector3 BoardToWorldPosition(Vector2Int boardPosition)
        {
            return new Vector3(
                boardPosition.x + _worldZeroPosition.x,
                boardPosition.y + _worldZeroPosition.y
            );
        }

        private Vector2Int WorldToBoardPosition(Vector3 worldPosition)
        {
            return new Vector2Int(
                Mathf.RoundToInt(worldPosition.x - _worldZeroPosition.x),
                Mathf.RoundToInt(worldPosition.y - _worldZeroPosition.y)
            );
        }

        private void PlaceTile(TileSO tileSO, Vector2Int position)
        {
            Tile tile = null;

            if (_inactiveTiles[tileSO].Count > 0)
            {
                tile = _inactiveTiles[tileSO].Pop();
            }
            else
            {
                tile = Instantiate(tileSO.Prefab, transform);
            }

            tile.Place(BoardToWorldPosition(position), tileSO, position);
            _tiles.Add(tile);
            _tilesByPosition.Add(position, tile);
            if (tile.Data.PuedeFusionarse) _standardTiles.Add(tile);
        }

        private void RemoveTile(Tile tile)
        {
            _inactiveTiles[tile.Data].Push(tile);
            _tilesByPosition.Remove(tile.BoardPosition);
            _tiles.Remove(tile);
            if (tile.Data.PuedeFusionarse) _standardTiles.Remove(tile);
        }

        private void TrySlideTile(Vector2Int position, Vector2Int direction)
        {
            Tile tile = _tilesByPosition[position];
            if (!tile.Data.CanSlide) return;
            Tile tileToMergeWith = null;
            tile.NextBoardPosition = position;
            Vector2Int nextPosition = position + direction;

            while (nextPosition.x >= 0 && nextPosition.x < _boardSize.x
                && nextPosition.y >= 0 && nextPosition.y < _boardSize.y)
            {
                tile.NextBoardPosition = nextPosition;

                if (_tilesByPosition.ContainsKey(nextPosition))
                {
                    Tile otherTile = _tilesByPosition[nextPosition];

                    if (tile.Data.BaseNumber == otherTile.Data.BaseNumber && tile.NextPower == otherTile.NextPower
                        && tile.Data.PuedeFusionarse && otherTile.Data.PuedeFusionarse)
                    {
                        tileToMergeWith = otherTile;
                    }
                    else tile.NextBoardPosition = nextPosition - direction;

                    break;
                }
                else nextPosition += direction;
            }

            _tilesByPosition.Remove(tile.BoardPosition);

            if (tileToMergeWith) FusionarMosaicos(tile, tileToMergeWith);
            else _tilesByPosition.Add(tile.NextBoardPosition, tile);

            tile.Move(BoardToWorldPosition(tile.NextBoardPosition), tile.NextBoardPosition);
        }

        private void FusionarMosaicos(Tile tile1, Tile tile2)
        {
            tile1.RemoveAfterMove = true;
            tile2.NextPower++;
            Vector3 mergePosition = tile2.transform.position;
            mergePosition.z = -tile2.NextPower;
            tile2.transform.position = mergePosition;
            RemoveTile(tile1);
        }

        private void CheckIfGameEnded()
        {
            if (_standardTiles.Count == 0) 
            {
                Debug.Log("tiempo para acabarlo" + ":" + timer);
                                
           
                if (_levelSOs[LevelSOIndex] == _levelSOs[0]  )
                    PlayerPrefs.SetFloat("record" + 1, timer);
                    if ( timer < PlayerPrefs.GetFloat("record" + 1,0))
                    {
                        PlayerPrefs.SetFloat("record" + 1, timer);
                    }
                   
                if (_levelSOs[LevelSOIndex] == _levelSOs[1])
                    PlayerPrefs.SetFloat("record" + 2, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 2, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 2, timer);
                    }              
                if (_levelSOs[LevelSOIndex] == _levelSOs[2])
                    PlayerPrefs.SetFloat("record" + 3, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 3, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 3, timer);
                    }                             
                if (_levelSOs[LevelSOIndex] == _levelSOs[3])
                    PlayerPrefs.SetFloat("record" + 4, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 4, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 4, timer);
                    }               
                if (_levelSOs[LevelSOIndex] == _levelSOs[4])
                    PlayerPrefs.SetFloat("record" + 5, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 5, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 5, timer);
                    }                
                if (_levelSOs[LevelSOIndex] == _levelSOs[5])
                    PlayerPrefs.SetFloat("record" + 6, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 6, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 6, timer);
                    }             
                if (_levelSOs[LevelSOIndex] == _levelSOs[6])
                    PlayerPrefs.SetFloat("record" + 7, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 7, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 7, timer);
                    }                
                if (_levelSOs[LevelSOIndex] == _levelSOs[7])
                    PlayerPrefs.SetFloat("record" + 8, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 8, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 8, timer);
                    }
                if (_levelSOs[LevelSOIndex] == _levelSOs[8])
                    PlayerPrefs.SetFloat("record" + 9, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 9, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 9, timer);
                    }
                if (_levelSOs[LevelSOIndex] == _levelSOs[9])
                    PlayerPrefs.SetFloat("record" + 10, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 10, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 10, timer);
                    }
                if (_levelSOs[LevelSOIndex] == _levelSOs[10])
                    PlayerPrefs.SetFloat("record" + 11, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 11, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 11, timer);
                    }
                if (_levelSOs[LevelSOIndex] == _levelSOs[11])
                    PlayerPrefs.SetFloat("record" + 12, timer);
                    if (timer < PlayerPrefs.GetFloat("record" + 12, 0))
                    {
                        PlayerPrefs.SetFloat("record" + 12, timer);
                    }




                _sceneTransition.TransitionTo("Level Completed");

                
            }



        }
        #endregion

        #region Public Methods
        public void SlideTiles(Vector2Int direction)
        {
            if (!Mathf.Approximately(_slideCooldownTimer, 0)) return;
            _slideCooldownTimer = _slideCooldown;

            if (direction.x == -1)
            {
                for (int x = 1; x < _boardSize.x; x++)
                {
                    for (int y = 0; y < _boardSize.y; y++)
                    {
                        Vector2Int position = new Vector2Int(x, y);
                        if (!_tilesByPosition.ContainsKey(position)) continue;
                        TrySlideTile(position, direction);
                    }
                }
            }
            else if (direction.x == 1)
            {
                for (int x = _boardSize.x - 2; x >= 0; x--)
                {
                    for (int y = 0; y < _boardSize.y; y++)
                    {
                        Vector2Int position = new Vector2Int(x, y);
                        if (!_tilesByPosition.ContainsKey(position)) continue;
                        TrySlideTile(position, direction);
                    }
                }
            }
            else if (direction.y == -1)
            {
                for (int y = 1; y < _boardSize.y; y++)
                {
                    for (int x = 0; x < _boardSize.x; x++)
                    {
                        Vector2Int position = new Vector2Int(x, y);
                        if (!_tilesByPosition.ContainsKey(position)) continue;
                        TrySlideTile(position, direction);
                    }
                }
            }
            else if (direction.y == 1)
            {
                for (int y = _boardSize.y - 2; y >= 0; y--)
                {
                    for (int x = 0; x < _boardSize.x; x++)
                    {
                        Vector2Int position = new Vector2Int(x, y);
                        if (!_tilesByPosition.ContainsKey(position)) continue;
                        TrySlideTile(position, direction);
                    }
                }
            }

            for (int i = _tiles.Count - 1; i >= 0; i--)
            {
                Tile tile = _tiles[i];

                if (tile.NextPower != tile.Power)
                {
                    if (tile.SetPower(tile.NextPower))
                    {
                        RemoveTile(tile);
                        CheckIfGameEnded();
                    }
                }
            }
        }

        public void Retry()
        {
            if (_isRetrying) return;

            for (int i = _tiles.Count - 1; i >= 0; i--)
            {
                Tile tile = _tiles[i];
                tile.Remove();
                RemoveTile(tile);
            }

            _isRetrying = true;
            _retryCooldownTimer = _retryCooldown;
        }
        #endregion
    }
}
