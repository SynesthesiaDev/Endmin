// Copyright (c) 2026 SynesthesiaDev <synesthesiadev@proton.me>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Codon.Codec;
using Serilog;
using SynesthesiaDev.Synx;
using SynesthesiaDev.Synx.Codon;

namespace Endmin;

public class HashesFile
{
    public static Dictionary<string, string> Hashes => contents.Hashes;

    private static Contents contents = Contents.DEFAULT;

    public static void ReadFile()
    {
        Log.Verbose("Loading hashes file..");
        if (!File.Exists(ConfigurationManager.HASH_FILE))
        {
            writeInternalDefault();
        }
        else
        {
            var text = File.ReadAllText(ConfigurationManager.HASH_FILE);
            var decoded = Contents.CODEC.Decode(SynxTranscoder.INSTANCE, text.ToSynxObject());
            contents = decoded;
        }
    }

    public static void WriteFile()
    {
        Log.Verbose("Writing hashes file..");

        if (!File.Exists(ConfigurationManager.HASH_FILE))
            writeInternalDefault();

        var encoded = Contents.CODEC.Encode(SynxTranscoder.INSTANCE, contents);
        File.WriteAllText(ConfigurationManager.HASH_FILE, encoded.Object().EncodeToString());
    }

    private static void writeInternalDefault()
    {
        Log.Debug("Hash file not found, creating one..");
        File.Create(ConfigurationManager.HASH_FILE).Close();
        var hashFile = Contents.CODEC.Encode(SynxTranscoder.INSTANCE, Contents.DEFAULT).Object().EncodeToString();

        File.WriteAllText(ConfigurationManager.HASH_FILE, hashFile);

    }

    private record Contents(Dictionary<string, string> Hashes)
    {
        internal static readonly Contents DEFAULT = new Contents([]);

        internal static readonly Codec<Contents> CODEC = StructCodec.For<Contents>()
            .Field("Hashes", Codecs.STRING.MapTo(Codecs.STRING), c => c.Hashes)
            .Build(hashes => new Contents(hashes));
    }
}
