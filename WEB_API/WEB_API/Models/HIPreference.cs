using System;

namespace XEuropeApp
{
     /**
     * This class defines the constraints for d-touch markers.It contains two sets of parameters. 
     * The first set defines the d-touch markers which needs to be identified. For example max & min
     * branches in a marker, empty branches and max leaves in a branch. 
     * The second set of parameters are used to define to validate a marker. It defines 
     * the number of validation branches, leaves in a validation branch and the checksum modulo.
     *    
     * @author pszsa1
     *
     */
    public class HIPreference
    {
        //branches default values
        protected int DEFAULT_MIN_BRANCHES = 2;
        protected int DEFAULT_MAX_BRANCHES = 5;
        protected int DEFAULT_MAX_EMPTY_BRANCHES = 1;
        protected int DEFAULT_VALIDATION_BRANCHES = 0;
        //leaves default values
        protected int DEFAULT_MAX_LEAVES = 50;
        protected int DEFAULT_VALIDATION_BRANCH_LEAVES =0;
        protected int DEFAULT_CHECKSUM_MODULO = 1;
        //marker occurrence value
        protected int DEFAULT_MARKER_OCCURRENCE = 1;

        //Minimum & Maximum number of branches.
        private static String MIN_BRANCHES = "min_branches";
        private static String MAX_BRANCHES = "max_branches";
        //Empty branches. A d-touch marker can not have all empty branches.
        private static String EMPTY_BRANCHES = "empty_branches";
        //Maximum leaves in a branch.
        private static String MAX_LEAVES = "max_leaves";
        //Maximum validation branches.
        private static String VALIDATION_BRANCHES = "validation_branches";
        //Maximum leaves in a validation branch.
        private static String VALIDATION_BRANCH_LEAVES = "validation_branch_leaves";
        //The total number of leaves in a marker should be divisible by the checksum modulo.
        private static String CHECKSUM_MODULO = "checksum_modulo";
        //Maximum number of marker occurrence.
        private static String MARKER_OCCURRENCE = "marker_occurrence";

        //protected Context mContext;

        public HIPreference()//Context context)
        {
            //this.mContext = context;
        }

        public int getMinBranches()
        {
            return GetSharedValue(MIN_BRANCHES, DEFAULT_MIN_BRANCHES);
        }

        public int getMaxBranches()
        {
            return GetSharedValue(MAX_BRANCHES, DEFAULT_MAX_BRANCHES);
        }

        public int getMaxEmptyBranches()
        {
            return GetSharedValue(EMPTY_BRANCHES, DEFAULT_MAX_EMPTY_BRANCHES);
        }

        public int getValidationBranches()
        {
            return GetSharedValue(VALIDATION_BRANCHES, DEFAULT_VALIDATION_BRANCHES);
        }

        public int getMaxLeaves()
        {
            return GetSharedValue(MAX_LEAVES, DEFAULT_MAX_LEAVES);
        }

        public int getValidationBranchLeaves()
        {
            return GetSharedValue(VALIDATION_BRANCH_LEAVES, DEFAULT_VALIDATION_BRANCH_LEAVES);
        }

        public int getChecksumModulo()
        {
            return GetSharedValue(CHECKSUM_MODULO, DEFAULT_CHECKSUM_MODULO);
        }

        public int getMarkerOccurrence()
        {
            return GetSharedValue(MARKER_OCCURRENCE, DEFAULT_MARKER_OCCURRENCE);
        }

        public void setDefaultMinBranches(int minBranches)
        {
            this.DEFAULT_MIN_BRANCHES = minBranches;
        }

        public void setDefaultMaxBranches(int maxBranches)
        {
            this.DEFAULT_MAX_BRANCHES = maxBranches;
        }

        public void setDefaultEmptyBranches(int emptyBranches)
        {
            this.DEFAULT_MAX_EMPTY_BRANCHES = emptyBranches;
        }

        public void setDefaultValidationBranches(int validationBranches)
        {
            this.DEFAULT_VALIDATION_BRANCHES = validationBranches;
        }

        public void setDefaultValidationBranchLeaves(int validationBranchLeaves)
        {
            this.DEFAULT_VALIDATION_BRANCH_LEAVES = validationBranchLeaves;
        }

        public void setDefaultMaxLeaves(int maxLeaves)
        {
            this.DEFAULT_MAX_LEAVES = maxLeaves;
        }

        public void setDefaultChecksumModulo(int checksumModulo)
        {
            this.DEFAULT_CHECKSUM_MODULO = checksumModulo;
        }

        public void setDefaultMarkerOccurrence(int markerOccurrence)
        {
            this.DEFAULT_MARKER_OCCURRENCE = markerOccurrence;
        }

        private int GetSharedValue(string key, int defaultValue)
        {
            int value = defaultValue;
            return value;
        }
    }
}
