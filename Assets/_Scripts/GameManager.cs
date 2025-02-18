using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region VARS
    public static GameManager Instance;

    [SerializeField] GameObject _chessPiece;
    [SerializeField] GameObject[,] _positions = new GameObject[8,8];
    [SerializeField] GameObject[] _playerBlack = new GameObject[16]; 
    [SerializeField] GameObject[] _playerWhite = new GameObject[16];
    [SerializeField] Team player;
    [SerializeField] bool _isGameOver;
    
    #endregion
    #region ENGINE
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #endregion
    #region MEMBER METHODS
    #endregion
    #region LOCAL METHODS
    #endregion
}
