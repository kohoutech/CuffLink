/* ----------------------------------------------------------------------------
Origami Lin32 Library
Copyright (C) 1997-2019  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Origami.Win32;

namespace CuffLink
{
    public class Linker
    {
        Options options;
        List<Win32Obj> objFiles;
        Win32Exe exefile;
        List<Section> sections;

        public Linker(Options opts)
        {
            options = opts;
            objFiles = new List<Win32Obj>();
            exefile = null;

            sections = new List<Section>();
            sections.Add(new Section(".text"));
            sections.Add(new Section(".data"));
            sections.Add(new Section(".rdata"));
            sections.Add(new Section(".bss"));
            sections.Add(new Section(".idata"));
            sections.Add(new Section(".edata"));
        }

        public void loadObjectFiles(List<string> filenames)
        {
            foreach (String fname in filenames)
            {
                Win32Obj objFile = new Win32Obj(fname);
                if (objFile != null)
                {
                    objFiles.Add(objFile);
                }
            }
        }

        public void link()
        {
            //throw new NotImplementedException();
        }

        public void writeExecutableFile(String exename)
        {
            exefile = new Win32Exe();
            exefile.layoutImage();
            exefile.writeFile(exename);
        }
    }
}
