using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block
{
    public virtual string Name => "Air";
    public virtual int BackFace => 0;
    public virtual int FrontFace => 0;
    public virtual int LeftFace => 0;
    public virtual int RightFace => 0;
    public virtual int TopFace => 0;
    public virtual int BottomFace => 0;
}

public class SolidBlock : Block 
{
    protected int _block;
    public override int BackFace => _block;
    public override int FrontFace => _block;
    public override int LeftFace => _block;
    public override int RightFace => _block;
    public override int TopFace => _block;
    public override int BottomFace => _block;
}

public sealed class Bedrock : SolidBlock
{
    public Bedrock() =>  _block = 5;

    public override string Name => "Bedrock";
}

public sealed class Stone : SolidBlock
{
    public Stone() => _block = 4;
    public override string Name => "Stone";
}

public sealed class Dirt : SolidBlock
{
    public Dirt() => _block = 3;
    public override string Name => "Dirt";
}

public sealed class Grass : SolidBlock
{
    public Grass() => _block = 2;
    public override string Name => "Grass";
    public override int TopFace => 1;
    public override int BottomFace => 3;
}

public sealed class Sand : SolidBlock
{
    public Sand() => _block = 6;
    public override string Name => "Sand";
}
