using System.Collections;
using Sgtin.Enums;

namespace Sgtin;

public class SgtinProcessor
{
    private TagType Type { get; set; }

    public SgtinProcessor(TagType type)
    {
        Type = type;
    }

    public static SgtinTagInfo Decode(string tag)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(tag);
        if (tag.Length % 2 != 0)
        {
            throw new ArgumentOutOfRangeException("Tag malformed. Length should be odd.");
        }

        return Decode((ReadOnlySpan<char>)tag);
    }

    public static string Encode(SgtinTagInfo tagInfo)
    {
        return Encode(tagInfo.Type, tagInfo.Filter, tagInfo.CompanyPrefix, tagInfo.ItemReference, tagInfo.Serial, partition: tagInfo.Partition);
    }

    public static string Encode(TagType type, int filter, ulong companyPrefix, ulong itemReference, ulong itemSerial,
        int companyPrefixLength = -1, int partition = -1)
    {
        int totalTagLength = 0;
        switch (type)
        {
            case TagType.Sgtin96:
                totalTagLength = 96;
                break;
            case TagType.Sgtin198:
                totalTagLength = 198;
                break;
            default:
                throw new ArgumentOutOfRangeException("Unsupported tag type");
        }

        var tagBits = new BitArray(totalTagLength);
        var tagType = ReverseBitArray(new BitArray(new byte[]{(byte)type}));
        for (int i = 0; i < tagType.Length; i++)
        {
            tagBits[i] = tagType[i];
        }

        var tagFilter = new BitArray(new byte[] { (byte)filter });
        for (int i = 2, j = 0; i >=0; i--, j++)
        {
            tagBits[8 + j] = tagFilter[i];
        }

        var partitionValue = partition > -1 ? partition :
            PartitionFilterValues.GetPartitionFromCompanyPrefix(companyPrefixLength > -1 ? companyPrefixLength : companyPrefix.ToString().Length);
        var partitionBits = new BitArray(new byte[] { (byte)partitionValue });
        for (int i = 2, j = 0; i >= 0; i--, j++)
        {
            tagBits[11 + j] = partitionBits[i];
        }

        var companyPrefixLsb = PartitionFilterValues.Sgtin96PartitionMap[partitionValue][0];
        var companyPrefixBits = new BitArray(BitConverter.GetBytes(companyPrefix));

        for (int i = companyPrefixLsb - 1, j = 0; i >= 0; i--, j++)
        {
            tagBits[14 + j] = companyPrefixBits[i];
        }

        var itemRefernceLsb = PartitionFilterValues.Sgtin96PartitionMap[partitionValue][1];
        var itemReferenceBits = new BitArray(BitConverter.GetBytes(itemReference));
        
        for (int i = itemRefernceLsb - 1, j = 0; i >= 0; i--, j++)
        {
            tagBits[14 + companyPrefixLsb + j] = itemReferenceBits[i];
        }
        
        var itemSerialBits = new BitArray(BitConverter.GetBytes(itemSerial));
        for (int i = 37, j = 0; i >= 0; i--, j++)
        {
            tagBits[14 + companyPrefixLsb + itemRefernceLsb + j] = itemSerialBits[i];
        }

        var byteArray = new byte[12]; 
        tagBits.CopyTo(byteArray, 0);
        
        
        
        var flipped = FlipOctetsFromByteArray(byteArray);
        bool[] debugBits = new bool[tagBits.Length];
        tagBits.CopyTo(debugBits, 0);
        string[] debugChars = debugBits.Select(x => x ? "1" : "0").ToArray();
        Console.WriteLine(string.Join("", debugChars));

        byte[] resultBytes = new byte[(int)Math.Ceiling(totalTagLength / 8m)];
        flipped.CopyTo(resultBytes, 0);
        var result = Convert.ToHexString(resultBytes);
        return result;
    }

    private static SgtinTagInfo Decode(ReadOnlySpan<char> tag)
    {
        var tagBytes = Convert.FromHexString(tag);
        TagType tagType;

        // since first byte is header type, we can process it immediately and do basic validation:
        switch (tagBytes[0])
        {
            case (int)TagType.Sgtin96:
                if (tagBytes.Length != 12)
                {
                    throw new ArgumentOutOfRangeException("Invalid tag");
                }

                tagType = TagType.Sgtin96;
                break;
            case (int)TagType.Sgtin198:
                if (tagBytes.Length != 25)
                {
                    throw new ArgumentOutOfRangeException("Invalid tag");
                }

                tagType = TagType.Sgtin198;
                break;

            default:
                throw new NotSupportedException("Tag type not supported");
        }

        var tagBits = FlipOctetsFromByteArray(tagBytes);

        // we need partition for processing rest of data
        var partition = (int)GetNumericValueFromTag(tagBits, 11, 14);
        var companyPrefixEnd = 14 + PartitionFilterValues.Sgtin96PartitionMap[partition][0];
        var itemReferenceEnd = companyPrefixEnd + PartitionFilterValues.Sgtin96PartitionMap[partition][1];

        SgtinTagInfo result = new()
        {
            Type = tagType,
            Filter = (int)GetNumericValueFromTag(tagBits, 8, 11),
            Partition = partition,
            CompanyPrefix = GetNumericValueFromTag(tagBits, 14, companyPrefixEnd),
            ItemReference = GetNumericValueFromTag(tagBits, companyPrefixEnd, itemReferenceEnd),
            Serial = GetNumericValueFromTag(tagBits, itemReferenceEnd, tagBits.Length)
        };

        bool[] debugBits = new bool[tagBits.Length];
        tagBits.CopyTo(debugBits, 0);
        string[] debugChars = debugBits.Select(x => x ? "1" : "0").ToArray();
        Console.WriteLine(string.Join("", debugChars));
        Console.WriteLine(result);

        Console.WriteLine(tag.ToString());
        Console.WriteLine(Encode(result));
        return result;
    }

    private static BitArray FlipOctetsFromByteArray(byte[] input)
    {
        BitArray result = new(input.Length * 8);
        for (int i = 0; i < input.Length; i++)
        {
            var currentByteBits = new BitArray(new byte[] { input[i] });
            var currentByteBitsR = ReverseBitArray(currentByteBits);
            for (int j = 0; j < currentByteBitsR.Length; j++)
            {
                result[i * 8 + j] = currentByteBitsR[j];
            }
        }

        return result;
    }

    private static BitArray ReverseBitArray(BitArray input)
    {
        var reversed = new BitArray(input);

        for (int i = 0, j = input.Length - 1; i < input.Length; ++i, --j)
        {
            input[i] ^= reversed[j];
            reversed[j] ^= input[i];
            input[i] ^= reversed[j];
        }

        return reversed;
    }

    private static ulong GetNumericValueFromTag(BitArray input, int startIndex, int endIndex)
    {
        ulong n = 0;
        BitArray tagPartBitArray = new(endIndex - startIndex);
        for (int i = startIndex; i < endIndex; i++)
        {
            tagPartBitArray[i - startIndex] = input[i];
        }

        tagPartBitArray = ReverseBitArray(tagPartBitArray);

        for (int i = 0; i < tagPartBitArray.Length; i++)
        {
            if (tagPartBitArray.Get(i))
            {
                n |= 1UL << i;
            }
        }

        return n;
    }
}