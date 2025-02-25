using UnityEngine;

public class NetMessage
{
    public EOpCode OpCode { set; get; }
}
public enum EOpCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH = 5
}
