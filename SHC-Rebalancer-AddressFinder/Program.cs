var filePath = "C:\\Program Files (x86)\\Stronghold Crusader\\Stronghold Crusader.exe";

var pattern = new List<byte> { 5, 235, 62, 131, 248, 71, 117, 5, 141, 104, 190, 235, 52, 131, 248, 72, 117, 5 };

try
{
    var fileBytes = File.ReadAllBytes(filePath);

    for (var i = 0; i <= fileBytes.Length - pattern.Count; i++)
    {
        var matchFound = true;

        for (var j = 0; j < pattern.Count; j++)
        {
            if (fileBytes[i + j] != pattern[j])
            {
                matchFound = false;
                break;
            }
        }

        if (matchFound)
        {
            Console.WriteLine("Sequence found at address 0x{0:X}:", i);
            for (var k = 0; k < pattern.Count; k++)
                Console.WriteLine("0x{0:X}: {1}", i + k, pattern[k]);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error occured: " + ex.Message);
}
/*
var pattern = new List<int> { 75, 5, 12, 60, 80, 80, 100 };
var intSize = sizeof(int);

try
{
    var fileBytes = File.ReadAllBytes(filePath);

    for (var i = 0; i <= fileBytes.Length - pattern.Count * intSize; i += intSize)
    {
        var matchFound = true;

        for (var j = 0; j < pattern.Count; j++)
        {
            var value = BitConverter.ToInt32(fileBytes, i + j * intSize);
            if (value != pattern[j])
            {
                matchFound = false;
                break;
            }
        }

        if (matchFound)
        {
            Console.WriteLine("Sequence found at address 0x{0:X}:", i);
            for (var k = 0; k < pattern.Count; k++)
                Console.WriteLine("0x{0:X}: {1}", i + k * intSize, pattern[k]);
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error occured: " + ex.Message);
}
*/