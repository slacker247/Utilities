using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Utilities.file_io
{
    public class FileInfoCompare : IComparer<FileInfo>
    {
        public const int DATE_CREATED = 0;
        public const int FILE_SIZE = 1;
        public const int DATE_LAST_MODIFIED = 2;

        protected int m_SortField = 2;
        public int SortField
        {
            get
            {
                return m_SortField;
            }
            set
            {
                m_SortField = value;
            }
        }

        public int Compare(FileInfo lhs, FileInfo rhs)
        {
            int status = -1;
            switch (SortField)
            {
                case FileInfoCompare.DATE_CREATED:
                    if (rhs.CreationTime == lhs.CreationTime)
                    {
                        status = 0;
                    }
                    else if (rhs.CreationTime < lhs.CreationTime)
                    {
                        status = 1;
                    }
                    else
                    {
                        status = -1;
                    }
                    break;
                case FileInfoCompare.DATE_LAST_MODIFIED:
                    if (rhs.LastWriteTime == lhs.LastWriteTime)
                    {
                        status = 0;
                    }
                    else if (rhs.LastWriteTime < lhs.LastWriteTime)
                    {
                        status = 1;
                    }
                    else
                    {
                        status = -1;
                    }
                    break;
                case FileInfoCompare.FILE_SIZE:
                    if (rhs.Length == lhs.Length)
                    {
                        status = 0;
                    }
                    else if (rhs.Length < lhs.Length)
                    {
                        status = 1;
                    }
                    else
                    {
                        status = -1;
                    }
                    break;
            }
            return status;
        }
    }
}
