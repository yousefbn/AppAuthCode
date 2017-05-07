using Java.Lang;
using System.Collections.Generic;
using System;


namespace ClassLibrary1 { 
public class FixedStack<T> : Stack<T>
{
    int maxSize;


    public int getMaxSize()
    {
        return maxSize;
    }

    public void setMaxSize(int maxSize)
    {
        this.maxSize = maxSize;
    }






}

}
}