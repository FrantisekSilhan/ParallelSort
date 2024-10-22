using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

public class SortingBenchmark {
    private int[] array;
    private const int ArraySize = 100000;
    private const int Threads = 12;

    [GlobalSetup]
    public void Setup() {
        array = shuffleArray(generateArray(ArraySize));
    }

    private int[] generateArray(int size) {
        int[] array = new int[size];
        for (int i = 0; i < size; i++) {
            array[i] = i;
        }
        return array;
    }

    private int[] shuffleArray(int[] array) {
        Random random = new Random();
        for (int i = 0; i < array.Length; i++) {
            int j = random.Next(i, array.Length);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
        return array;
    }

    private int[] sortArraySync(int[] array) {
        int[] sortedArray = (int[])array.Clone();
        for (int i = 1; i < sortedArray.Length; i++) {
            int key = sortedArray[i];
            int j = i - 1;
            while (j >= 0 && sortedArray[j] > key) {
                sortedArray[j + 1] = sortedArray[j];
                j = j - 1;
            }
            sortedArray[j + 1] = key;
        }

        return sortedArray;
    }

    private int[] sortArrayAsync(int[] array, int threads) {
        int[] sortedArray = new int[array.Length];
        int chunkSize = array.Length / threads;
        Task[] tasks = new Task[threads];
        int[][] sortedChunks = new int[threads][];

        for (int i = 0; i < threads; i++) {
            int start = i * chunkSize;
            int end = i == threads - 1 ? array.Length : (i + 1) * chunkSize;
            int threadIndex = i;

            tasks[i] = Task.Run(() => {
                int[] chunk = new int[end - start];
                Array.Copy(array, start, chunk, 0, end - start);
                for (int j = 1; j < chunk.Length; j++) {
                    int key = chunk[j];
                    int k = j - 1;
                    while (k >= 0 && chunk[k] > key) {
                        chunk[k + 1] = chunk[k];
                        k = k - 1;
                    }
                    chunk[k + 1] = key;
                }
                sortedChunks[threadIndex] = chunk;
            });
        }

        Task.WaitAll(tasks);

        int[] chunkIndices = new int[threads];
        for (int i = 0; i < array.Length; i++) {
            int minChunk = -1;
            int minValue = int.MaxValue;

            for (int j = 0; j < threads; j++) {
                if (chunkIndices[j] < sortedChunks[j].Length && sortedChunks[j][chunkIndices[j]] < minValue) {
                    minValue = sortedChunks[j][chunkIndices[j]];
                    minChunk = j;
                }
            }

            sortedArray[i] = minValue;
            chunkIndices[minChunk]++;
        }

        return sortedArray;
    }

    [Benchmark]
    public int[] BenchmarkSortSync() {
        return sortArraySync(array);
    }

    [Benchmark]
    public int[] BenchmarkSortAsync() {
        return sortArrayAsync(array, Threads);
    }
}

public class Program {
    public static void Main(string[] args) {
        var summary = BenchmarkRunner.Run<SortingBenchmark>();
    }
}
