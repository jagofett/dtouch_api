using System;
using System.Collections.Generic;
using OpenCvSharp.CPlusPlus;

//import org.opencv.core.Mat;

namespace XEuropeApp
{
    /**
    * 	INVALID Either branch does not have 0 or more dark regions or the regions inside the branch are not valid leaves.
    *	EMPTY if branch has zero leaves.
    *	VALID if branch has one or more leaves.
    * @author pszsa1
    */
    enum BranchStatus
    {
        INVALID,
        EMPTY,
        VALID
    }
    /**
     * Marker Detector class is used to identify d-touch markers. 
     * @author pszsa1
     *
     */
    public class MarkerDetector
    {
        //indexes of leaf nodes in contour tree hierarchy.
        private static readonly int NEXT_NODE = 0;
        private static readonly int FIRST_NODE = 2;

        private HIPreference mPreference;

        /**
         * MarkerDetector Constructor
         * @param context   The Activity or application context.
         * @param preference Preference object to use default preference values and to check minimum and maximum value constraints. 
         */
        public MarkerDetector(HIPreference preference)
        {
            mPreference = preference;
        }

        /**
         * This function detects whether a particular node is a d-touch marker. A d-touch contains one dark (black) region which is called the root. A root can have 
         * one or more light (white) regions. These light regions are called branches. Each branch can contain zero or more dark (black) regions and these dark regions
         * are called leaves. If a branch has no leaf then it is called an empty branch. In conclusion Root==> 1 or more branches ==> 0 or more leaves.
         * 
         * @param rootIndex The index of the node which needs to be identified as the root. It is used to access the hierarchy of this node from the hierarchy parameter.	 * 
         * @param rootNode The node which needs to be checked as the root.   
         * @param hierarchy This contains the contours or components hierarchy using opencv findContours function.
         * @param codes An empty but not null list. It contains the code if the root node is determined as the d-touch marker.
         * @return returns true if the root node is a valid d-touch marker otherwise returns false.
         */
        public Boolean verifyRoot(int rootIndex, List<Vec4i> hierarchy, List<Int32> codes)
        {
            Boolean valid = false;
            int branchCount = 0;
            int emptyBranchCount = 0;
            int currentBranchIndex = -1;
            BranchStatus status;

            //get the nodes of the root node.
            var nodes = hierarchy[rootIndex];

            //get the first child node.
            currentBranchIndex = nodes[FIRST_NODE];

            //if there is a branch node then verify branches.
            if (currentBranchIndex >= 0)
            {
                //loop until there is a branch node.
                while (currentBranchIndex >= 0)
                {
                    //verify current branch.
                    status = verifyBranch(currentBranchIndex, hierarchy, codes);
                    //if branch is valid or empty.
                    if (status == BranchStatus.VALID || status == BranchStatus.EMPTY)
                    {
                        branchCount++;
                        if (status == BranchStatus.EMPTY)
                        {
                            emptyBranchCount++;
                            if (emptyBranchCount > mPreference.getMaxEmptyBranches())
                                return false;
                        }
                        //get next node. 
                        nodes = hierarchy[currentBranchIndex];
                        //hierarchy.GetArray(0, currentBranchIndex, nodes);
                        currentBranchIndex = (int)nodes[NEXT_NODE];
                    }
                    else if (status == BranchStatus.INVALID)
                    {
                        return false;
                    }
                }
                if (emptyBranchCount > mPreference.getMaxEmptyBranches())
                {
                    valid = false;
                }
                else if ((emptyBranchCount - branchCount) == 0)
                {
                    //Marker should have at least one non-empty branch. If all branches are empty then return false.
                    valid = false;
                }
                else if (branchCount >= mPreference.getMinBranches() && branchCount <= mPreference.getMaxBranches())
                {
                    if (verifyMarkerConstraint(codes))
                    {
                       codes.Sort();
                        //Collections.sort(codes);
                        valid = true;
                    }
                    else
                    {
                        valid = false;
                    }
                }
            }
            return valid;
        }

        /**
         * This function determines whether the input node is a valid branch. It is a valid branch if it contains zero or more dark regions.
         * @param branchIndex Node index from the hierarchy.
         * @param hierarchy This contains the contours or components hierarchy.
         * @param codes A list which holds marker code.
         * @return Returns branch status.
         */
        private BranchStatus verifyBranch(int branchIndex, List<Vec4i> hierarchy, List<Int32> codes)
        {
            int leafCount = 0;
            int currentLeafIndex = -1;
            BranchStatus status = BranchStatus.INVALID;

            //Vec4i test;

            //get first leaf node.
            //double[] nodes = new double[4];
            var nodes = hierarchy[branchIndex];
            //hierarchy.GetArray(0, branchIndex, nodes);
            currentLeafIndex = (int)nodes[FIRST_NODE];
            if (currentLeafIndex >= 0)
            {
                //loop until there is a leaf node.
                while (currentLeafIndex >= 0)
                {
                    if (verifyLeaf(currentLeafIndex, hierarchy))
                    {
                        leafCount++;
                        //get next leaf node.
                        //nodes = hierarchy.GetArray(0, currentLeafIndex);
                        nodes = hierarchy[currentLeafIndex];
                        currentLeafIndex = (int)nodes[NEXT_NODE];
                    }
                    else
                    {
                        status = BranchStatus.INVALID;
                        return status;
                    }
                }
            }

            //if no leaf then the branch is empty.
            if (leafCount == 0)
                status = BranchStatus.EMPTY;
            else if (leafCount <= mPreference.getMaxLeaves())
                status = BranchStatus.VALID;

            //add leaf count in branch code. Only add it when leaf count is greater than 0.
            if (leafCount > 0)
                codes.Add(leafCount);

            return status;
        }

        /**
         * This functions determines if the node is a valid leaf. It is a valid leaf if it does not have any child nodes.
         * @param leafIndex
         * @param hierarchy
         * @return
         */
        private Boolean verifyLeaf(int leafIndex, List<Vec4i> hierarchy)
        {
            Boolean valid = true;
            //Get nodes of branch index.
            //double[] nodes = new double[4];
            var nodes = hierarchy[leafIndex];
            //hierarchy.GetArray(0, leafIndex, nodes);
            //check if there is no child node.
            if (nodes[FIRST_NODE] >= 0)
                valid = false;

            return valid;
        }

        private Boolean verifyMarkerConstraint(List<Int32> codes)
        {
            MarkerConstraint markerConstraint = new MarkerConstraint(mPreference, codes);
            return markerConstraint.verifyMarkerCode();
        }


        /**
         * This function iterates through the list of markers which are identified as the valid markers. It filters 
         * the list based on the marker occurrence value specified in the preference. For example if marker occurrence preference 
         * is 2 then it selects only those markers having 2 or more instances in the markers list. It is kind of validation check.  
         * @param markers List of markers which are identified as valid markers. 
         * @return DtouchMarker which has occurred most in the input list.
         */
        public DtouchMarker compareDetectedMarkers(List<DtouchMarker> markers)
        {
            Dictionary<String, Int32> map = new Dictionary<String, Int32>();
            DtouchMarker markerSelected = null;
            //Record occurrence of each marker.
            foreach (DtouchMarker marker in markers)
            {
                if (map.ContainsKey(marker.getCodeKey()))
                {
                    Int32 value = map[marker.getCodeKey()];
                    value = value + 1;
                    map.Add(marker.getCodeKey(), value);
                }
                else
                {
                    map.Add(marker.getCodeKey(), 1);
                }
            }
            //Find out the marker with the occurrences equal or more than the value specified in the preference.
            KeyValuePair<String, Int32> maxMarker = new KeyValuePair<string, int>("", 0);
            foreach (var entry in map)
            {
                if ((maxMarker.Key == "" && maxMarker.Value == 0) && (entry.Value >= mPreference.getMarkerOccurrence()))
                    maxMarker = entry;
                else if (!(maxMarker.Key == "" && maxMarker.Value == 0) &&
                    (maxMarker.Value < entry.Value) &&
                    (entry.Value >= mPreference.getMarkerOccurrence()))
                    maxMarker = entry;
            }

            if (!(maxMarker.Key == "" && maxMarker.Value == 0))
            {
                //Find out the identified marker in the original list.
                foreach (DtouchMarker marker in markers)
                {
                    if (marker.getCodeKey().CompareTo(maxMarker.Key) == 0)
                    {
                        markerSelected = marker;
                        break;
                    }
                }
            }

            return markerSelected;
        }
    }
}
