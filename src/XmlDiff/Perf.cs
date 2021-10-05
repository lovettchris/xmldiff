//------------------------------------------------------------------------------
// <copyright file="Perf.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

#define MEASURE_PERF

namespace Microsoft.XmlDiffPatch {

#if MEASURE_PERF
    /// <summary>
    /// Class for tracking performance results.
    /// </summary>
    public class XmlDiffPerf 
    {
        internal int _loadTime = 0;
        internal int _hashValueComputeTime = 0;
        internal int _identicalOrNoDiffWriterTime = 0;
        internal int _matchTime = 0;
        internal int _preprocessTime = 0;
        internal int _treeDistanceTime = 0;
        internal int _diffgramGenerationTime = 0;
        internal int _diffgramSaveTime = 0;

        /// <summary>
        /// Total time measured.
        /// </summary>
        public int TotalTime { 
            get { 
                return _loadTime + _hashValueComputeTime + _identicalOrNoDiffWriterTime + _matchTime + _preprocessTime +
                    _treeDistanceTime + _diffgramGenerationTime + _diffgramSaveTime; 
            } 
        }

        /// <summary>
        /// Reset the counters.
        /// </summary>
        public void Clean() 
        {
            _loadTime = 0;
            _hashValueComputeTime = 0;
            _identicalOrNoDiffWriterTime = 0;
            _matchTime = 0;
            _preprocessTime = 0;
            _treeDistanceTime = 0;
            _diffgramGenerationTime = 0;
            _diffgramSaveTime = 0;
        }
    }
#endif
}