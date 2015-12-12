using System;
using System.Collections.Generic;

namespace XEuropeApp
{
    /**
 * This class holds the d-touch code and the index of the root component.
 * @author pszsa1
 *
 */
    public class DtouchMarker
    {
        private int mIndex;
        private List<Int32> mCode;

        //constructors
        public DtouchMarker()
        {
            //super();
        }

        public DtouchMarker(List<Int32> code)
        {
            //super();
            mCode = new List<Int32>(code);
        }

        public DtouchMarker(String code)
        {
            //super();
            this.setCode(code);
        }

        public int getComponentIndex()
        {
            return mIndex;
        }

        public void setComponentIndex(int componentIndex)
        {
            mIndex = componentIndex;
        }

        public List<Int32> getCode()
        {
            return mCode;
        }

        public void setCode(List<Int32> code)
        {
            if (mCode != null)
            {
                mCode.Clear();
                mCode = null;
            }
            mCode = new List<Int32>(code);
        }

        public void setCode(String code)
        {
            this.setCode(getCodeArrayFromString(code));
        }

        public String getCodeKey()
        {
            String codeKey = null;
            if (mCode != null)
                codeKey = codeArrayToString(mCode);
            return codeKey;
        }

        private String codeArrayToString(IEnumerable<int> codes)
        {
            return String.Join(":", codes);

            /*
             * var code = "";
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 0)
                    code += ":";
                code += codes[i];
            }
             * return code;
             * */
           
        }

        private static List<Int32> getCodeArrayFromString(String code)
        {
    	    string[] tmpCodes = code.Split(':');
    	    List<Int32> codes = new List<Int32>(); 
    	    for (int i = 0; i < tmpCodes.Length; i++)
            {
    		    codes.Add(Int32.Parse(tmpCodes[i]));
    	    }
    	    return codes;	
        }

        public bool isCodeEqual(DtouchMarker marker)
        {
            String thisCode = this.getCodeKey();
            String compareCode = marker.getCodeKey();

            if (thisCode.CompareTo(compareCode) == 0)
                return true;
            else
                return false;
        }

        // Object methods for use in Map, HashMap etc.
        public int hashCode()
        {
    	    int hash = 0;
    	    foreach (int i in mCode) 
            {
                hash += i;
            }

    	    return hash;
        }

        public bool equals(Object m)
        {
            if (m.GetType() != this.GetType())
            {
                return false;
            }
            return isCodeEqual((DtouchMarker)m);
        }

    }
}
