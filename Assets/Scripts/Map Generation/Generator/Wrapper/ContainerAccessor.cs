using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Common class to access common variables in GeneratorContainer
//      Each manager that needs access to anything in the GeneratorContainer needs to inherit this class
//      Only put the shared components here, accessor functions should be implemented in their respective files
//          Ex. TileManagerClass.cs should have a partial ContainerAccessor class 
public partial class ContainerAccessor
{
    GeneratorContainer contInst;
    public TileAccessor tileAccessor;

    public ContainerAccessor(ref GeneratorContainer contInst)
    {
        this.contInst = contInst;
        this.tileAccessor = new TileAccessor(ref contInst);
    }

    public ref GeneratorContainer getContainerInst()
    {
        return ref contInst;
    }
}

// Accessor functions that don't have a .cs file
public partial class ContainerAccessor
{
    public GameObject getGarbageGameObject()
    {
        return contInst.garbage;
    }
    
}


