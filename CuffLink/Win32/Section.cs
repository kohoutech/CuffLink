/* ----------------------------------------------------------------------------
Origami Win32 Library
Copyright (C) 1998-2019  George E Greaney

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

//https://en.wikibooks.org/wiki/X86_Disassembly/Windows_Executable_Files

namespace Origami.Win32
{
    public class Section
    {

        //section header fields
        public int secNum;
        public String name;

        public uint memloc;                 //section addr in memory
        public uint memsize;                //section size in memory
        public uint fileloc;                //section addr in file
        public uint filesize;               //section size in file

        public List<CoffRelocation> relocations;
        public List<CoffLineNumber> linenumbers;                //line num data is deprecated

        //flag fields
        public bool hasCode;
        public bool hasInitializedData;
        public bool hasUninitializedData;
        public bool hasInfo;
        public bool isRemoveable;
        public bool hasComdat;
        public bool resetSpecExcept;
        public bool hasGlobalPtrData;

        public uint imageBase;
        public byte[] data;

        //new section cons
        public Section(String _name)
        {
            secNum = 0;
            name = _name;

            memsize = 0;
            memloc = 0;
            filesize = 0;
            fileloc = 0;

            relocations = new List<CoffRelocation>();
            linenumbers = new List<CoffLineNumber>();

            flags = 0;
            imageBase = 0;
            data = new byte[0];
            relocTbl = new List<CoffRelocation>();
        }

        //loaded section cons
        public Section(int _secnum, String _secname, uint _memsize, uint _memloc, uint _filesize, uint _fileloc, 
            uint _pRelocations, uint _pLinenums, int _relocCount, int _linenumCount, uint _flags)
        {
            this.secNum = _secnum;
            this.name = _secname;

            this.memsize = _memsize;
            this.memloc = _memloc;
            this.filesize = _filesize;
            this.fileloc = _fileloc;

            this.pRelocations = _pRelocations;
            this.pLinenums = _pLinenums;
            this.relocCount = _relocCount;
            this.linenumCount = _linenumCount;

            this.flags = _flags;
            this.imageBase = 0;
            data = new byte[0];
            relocTbl = new List<CoffRelocation>();
        }

        internal void setData(byte[] _data)
        {
            data = _data;
        }

        internal void addReloc(CoffRelocation reloc)
        {
            relocTbl.Add(reloc);
        }

//- flag methods --------------------------------------------------------------

        public bool isCode()
        {
            return (flags & IMAGE_SCN_CNT_CODE) != 0;
        }

        public bool isInitializedData()
        {
            return (flags & IMAGE_SCN_CNT_INITIALIZED_DATA) != 0;
        }

        public bool isUninitializedData()
        {
            return (flags & IMAGE_SCN_CNT_UNINITIALIZED_DATA) != 0;
        }

        public bool isDiscardable()
        {
            return (flags & IMAGE_SCN_MEM_DISCARDABLE) != 0;
        }

        public bool isExecutable()
        {
            return (flags & IMAGE_SCN_MEM_EXECUTE) != 0;
        }

        public bool isReadable()
        {
            return (flags & IMAGE_SCN_MEM_READ) != 0;
        }

        public bool isWritable()
        {
            return (flags & IMAGE_SCN_MEM_WRITE) != 0;
        }

//- reading in ----------------------------------------------------------------

        public static Section loadSection(SourceFile source)
        {

            Section section = new Section();
            section.name = source.getAsciiString(8);

            section.memsize = source.getFour();
            section.memloc = source.getFour();
            section.filesize = source.getFour();
            section.fileloc = source.getFour();

            section.pRelocations = source.getFour();
            section.pLinenums = source.getFour();
            section.relocCount = (int)source.getTwo();
            section.linenumCount = (int)source.getTwo();
            section.flags = source.getFour();

            //load section data - read in all the bytes that will be loaded into mem (memsize)
            //and skip the remaining section bytes (filesize) to pad out the data to a file boundary
            section.data = source.getRange(section.fileloc, section.memsize);          

            return section;
        }

//- writing out ---------------------------------------------------------------


        internal void writeSectionTblEntry(OutputFile outfile)
        {
            outfile.putFixedString(name, 8);

            outfile.putFour(memsize);
            outfile.putFour(memloc);
            outfile.putFour(filesize);
            outfile.putFour(fileloc);

            outfile.putFour(pRelocations);
            outfile.putFour(0);
            outfile.putTwo((uint)relocTbl.Count);
            outfile.putTwo(0);

            outfile.putFour(flags);
        }

        internal void writeSectionData(OutputFile outfile)
        {
            outfile.putRange(data);
            if (relocTbl != null)
            {
                for (int i = 0; i < relocTbl.Count; i++)
                {
                    relocTbl[i].writeToFile(outfile);
                }
            }
        }
    }

//-----------------------------------------------------------------------------

    public class CoffRelocation
    {
        public enum Reloctype
        {
            ABSOLUTE = 0x00,
            DIR32 = 0x06,
            DIR32NB = 0x07,
            SECTION = 0x0a,
            SECREL = 0x0b,
            TOKEN = 0x0c,
            SECREL7 = 0x0d,
            REL32 = 0x14
        }

        public uint address;
        public uint symTblIdx;
        public Reloctype type;

        public CoffRelocation(uint _addr, uint _idx, Reloctype _type)
        {
            address = _addr;
            symTblIdx = _idx;
            type = _type;
        }

        internal void writeToFile(OutputFile outfile)
        {
            outfile.putFour(address);
            outfile.putFour(symTblIdx);
            outfile.putTwo((uint)type);            
        }
    }

    public class CoffLineNumber
    {
    }
}
