using System;
using System.Collections.Generic;

namespace Hofinsoft.Mdg.Services
{
    /// <summary>
    /// Adaptive lifecycle routing logic.
    /// Determines the approval pipeline based on Request_Type.
    /// </summary>
    public class LifecycleRouter
    {
        // 4-stage chain: Single or Multiple
        private static readonly string[] FourStageChain = new[]
        {
            "Requester",       // Stage 1
            "Approver",        // Stage 2 (Manager)
            "CentralCataloger", // Stage 3
            "CentralApprover"  // Stage 4
        };

        // 3-stage chain: Modification or Plant_Extension
        private static readonly string[] ThreeStageChain = new[]
        {
            "Requester",         // Stage 1
            "Approver",          // Stage 2 (Manager)
            "CentralCataloger"   // Stage 3
        };

        /// <summary>
        /// Returns the approval chain for the given request type.
        /// </summary>
        public string[] GetApprovalChain(string requestType)
        {
            return requestType switch
            {
                "Single" or "Multiple" => FourStageChain,
                "Modification" or "Plant_Extension" => ThreeStageChain,
                _ => FourStageChain // Default to 4-stage
            };
        }

        /// <summary>
        /// Returns the total number of stages for the given request type.
        /// </summary>
        public int GetTotalStages(string requestType)
        {
            return GetApprovalChain(requestType).Length;
        }

        /// <summary>
        /// Returns the role name for the given stage (1-based).
        /// </summary>
        public string GetRoleForStage(string requestType, int stage)
        {
            var chain = GetApprovalChain(requestType);
            if (stage < 1 || stage > chain.Length)
                throw new ArgumentOutOfRangeException(nameof(stage),
                    $"Stage {stage} is out of range for request type '{requestType}' (max: {chain.Length}).");
            return chain[stage - 1];
        }

        /// <summary>
        /// Determines if the given stage is the final stage for the request type.
        /// </summary>
        public bool IsFinalStage(string requestType, int stage)
        {
            return stage >= GetTotalStages(requestType);
        }

        /// <summary>
        /// Advances the lifecycle to the next stage. Returns updated stage, role, and whether it's now complete.
        /// </summary>
        public LifecycleAdvanceResult AdvanceStage(string requestType, int currentStage)
        {
            var chain = GetApprovalChain(requestType);
            var nextStage = currentStage + 1;

            if (nextStage > chain.Length)
            {
                return new LifecycleAdvanceResult
                {
                    NewStage = currentStage,
                    NewRole = chain[^1],
                    IsComplete = true,
                    Status = "Approved"
                };
            }

            return new LifecycleAdvanceResult
            {
                NewStage = nextStage,
                NewRole = chain[nextStage - 1],
                IsComplete = false,
                Status = "In_Progress"
            };
        }
    }

    public class LifecycleAdvanceResult
    {
        public int NewStage { get; set; }
        public string NewRole { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
