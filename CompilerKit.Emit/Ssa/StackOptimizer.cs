using System;

namespace CompilerKit.Emit.Ssa
{
    public static class StackOptimizer
    {
        public static Action<Body> Optimize { get; } = b => OptimizeImpl(b, new int[b.Count], -1);

        private static void OptimizeImpl(Body body, int[] keys, int key)
        {
            // A naive optimizer that can't optimize across jump
            // boundaries.
            var stack = new Variable[body.Count];
            var stackCount = 0;
            var visited = new int[body.Count];
            var visitedCount = 0;

            for (var i = 0; i < body.Count; i++)
            {
                int visitedIndex = 0;
                Check(body[i], ref stack, ref stackCount, visited, ref visitedIndex, ref visitedCount);
                visited[i] = 0;
            }
        }

        private static bool Check(Instruction v, ref Variable[] stack, ref int stackCount, int[] visited, ref int visitedIndex, ref int visitedCount)
        {
            // PERFORMANCE: Rework Tarjan's algo
            var foundBreak = false;
            for (var i = 0; i < visitedCount; i++)
            {
                var j = (visited.Length + visitedCount - i - 1) % visited.Length;
                if (visited[j] == 0) break;
                if (visited[j] == v.Index + 1)
                {
                    foundBreak = true;
                    break;
                }
            }

            visited[visitedCount] = v.Index + 1;
            visitedIndex = (visitedIndex + 1) % visited.Length;
            visitedCount++;

            // Recurse only if not visited already.
            if (!foundBreak && stackCount > 0)
            {
                var clone = (Variable[])stack.Clone();
                var originalStackCount = stackCount;
                var peek = stack[stackCount - 1];

                for (var i = 0; i < v.JumpsTo.Count; i++)
                {
                    AddOutputs(v, ref stack, ref stackCount);
                    var result = Check(v.JumpsTo[i], ref stack, ref stackCount, visited, ref visitedIndex, ref visitedCount);
                    result &= stackCount == 0 || peek != stack[stackCount - 1];

                    visited[i] = 0;
                    if (!result || i != v.JumpsTo.Count - 1)
                    {
                        stackCount = originalStackCount;
                        for (var j = 0; j < stackCount; j++)
                        {
                            stack[j] = clone[j];
                        }
                    }

                    if (!result)
                    {
                        AddOutputs(v, ref stack, ref stackCount);
                        return false;
                    }
                }
            }

            foreach (var input in v.InputVariables)
            {
                if ((input.Options & VariableOptions.StackOperations) != VariableOptions.None)
                    continue;

                if (input.IsParameter || stackCount == 0)
                {
                    input.Options |= VariableOptions.StackProhibited;
                    AddOutputs(v, ref stack, ref stackCount);
                    return false;
                }
                else
                {
                    var peek = stack[stackCount - 1];
                    if (peek != input || peek.Options.HasFlag(VariableOptions.StackProhibited))
                    {
                        peek.Options |= VariableOptions.StackProhibited;
                        input.Options |= VariableOptions.StackProhibited;
                    }
                    else
                    {
                        peek.Options |= VariableOptions.StackCandidate;
                        input.Options |= VariableOptions.StackCandidate;
                        stackCount--;
                    }
                }
            }

            AddOutputs(v, ref stack, ref stackCount);

            return true;
        }

        private static void AddOutputs(Instruction v, ref Variable[] stack, ref int stackCount)
        {
            var newCount = stackCount + v.OutputVariables.Count;
            if (stack.Length < newCount)
                Array.Resize(ref stack, newCount * 3 / 2);
            for (var i = v.OutputVariables.Count - 1; i >= 0; i--)
                stack[stackCount++] = v.OutputVariables[i];
        }
    }
}
