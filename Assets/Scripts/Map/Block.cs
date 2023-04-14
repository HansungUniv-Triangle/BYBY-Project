using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// юс╫ц Block class
[Serializable]
public class Block
{
    public enum BlockType {
        Air,
        Grass,
        Dirt,
        Stone,
        Sand,
        Wood,
        Leaf,
        Bedrock
    };

    public String Name;

    [SerializeField] private BlockType _type;
    [SerializeField] private bool _solidType;
    [SerializeField] private bool _transparencyType;
    [SerializeField] private int _maxHP;

    [Space(5f)]
    [SerializeField] private int _frontFaceTextureID;
    [SerializeField] private int _rightFaceTextureID;
    [SerializeField] private int _topFaceTextureID;
    [SerializeField] private int _backFaceTextureID;
    [SerializeField] private int _leftFaceTextureID;
    [SerializeField] private int _bottomFaceTextureID;

    public BlockType GetBlockType() 
    { 
        return _type; 
    }

    public bool GetSolidType()
    {
        return _solidType;
    }

    public bool GetTransparencyType()
    {
        return _transparencyType;
    }

    public int GetMaxHP()
    {
        return _maxHP;
    }

    public int GetTextureID(int faceNumber)
    {
        switch (faceNumber) 
        {
            case 0:  return _frontFaceTextureID;
            case 1:  return _rightFaceTextureID;
            case 2:  return _topFaceTextureID;
            case 3:  return _backFaceTextureID;
            case 4:  return _leftFaceTextureID;
            case 5:  return _bottomFaceTextureID;
            default: return -1;
        }
    }
}
