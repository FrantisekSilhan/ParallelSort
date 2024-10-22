using System.Diagnostics;

int[] generateArray(int size) {
    int[] array = new int[size];
    for (int i = 0; i < size; i++) {
        array[i] = i;
    }
    return array;
}

int[] shuffleArray(int[] array) {
    Random random = new Random();
    for (int i = 0; i < array.Length; i++) {
        int j = random.Next(i, array.Length);
        int temp = array[i];
        array[i] = array[j];
        array[j] = temp;
    }
    return array;
}

void printArray(int[] array) {
    for (int i = 0; i < array.Length; i++) {
        Console.Write(array[i] + " ");
    }
    Console.WriteLine();
}

int[] sortArraySync(int[] array) {
    for (int i = 1; i < array.Length; i++) {
        int key = array[i];
        int j = i - 1;
        while (j >= 0 && array[j] > key) {
            array[j + 1] = array[j];
            j = j - 1;
        }
        array[j + 1] = key;
    }

    return array;
}

int[] sortArrayAsync(int[] array, int threads) {
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


bool isSorted(int[] array) {
    for (int i = 1; i < array.Length; i++) {
        if (array[i] < array[i - 1]) {
            return false;
        }
    }
    return true;
}

Console.WriteLine("Insertion Sort!!! UwU");

int[] list = shuffleArray(generateArray(100000));

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

int[] sortedListAsync = sortArrayAsync(list, 12);

stopwatch.Stop();

Console.WriteLine("Async: " + stopwatch.ElapsedMilliseconds + "ms; Sorted: " + isSorted(sortedListAsync) + "; length: " + sortedListAsync.Length + "/" + list.Length);

stopwatch.Restart();

int[] sortedListSync = sortArraySync(list);

stopwatch.Stop();

Console.WriteLine("Sync: " + stopwatch.ElapsedMilliseconds + "ms; Sorted: " + isSorted(sortedListSync) + "; length: " + sortedListSync.Length + "/" + list.Length);