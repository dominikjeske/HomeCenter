using System;

namespace Wirehome.WindowsService.Interop
{
    public struct Blob
    {
        public int Length;
        public IntPtr Data;

        //Code Should Compile at warning level4 without any warnings, 
        //However this struct will give us Warning CS0649: Field [Fieldname] 
        //is never assigned to, and will always have its default value
        //You can disable CS0649 in the project options but that will disable
        //the warning for the whole project, it's a nice warning and we do want 
        //it in other places so we make a nice dummy function to keep the compiler
        //happy.
        private void FixCS0649()
        {
            Length = 0;
            Data = IntPtr.Zero;
        }
    }
}

