using GBX.NET;
using GBX.NET.BlockInfo;
using GBX.NET.Engines.Game;
using GBX.NET.Exceptions;
using System.Text.Json;



static BlockUnit[]? CreateVariant(CGameCtnBlockInfoVariant? blockInfoVariant)
{
    if (blockInfoVariant?.BlockUnitModels is null)
    {
        return null;
    }

    return blockInfoVariant
        .BlockUnitModels
        .OfType<CGameCtnBlockUnitInfo>()
        .Select(x => new BlockUnit
        {
            Coord = x.RelativeOffset,
            NorthClips = GetClipInfo(x.ClipsNorth),
            EastClips = GetClipInfo(x.ClipsEast),
            SouthClips = GetClipInfo(x.ClipsSouth),
            WestClips = GetClipInfo(x.ClipsWest),
            TopClips = GetClipInfo(x.ClipsTop),
            BottomClips = GetClipInfo(x.ClipsBottom)
        }).ToArray();
}

static string[]? GetClipInfo(ExternalNode<CGameCtnBlockInfoClip>[]? clips)
{
    return clips?.Length > 0
        ? clips.Select(x => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(x.File?.FileName)))
        .OfType<string>()
        .ToArray() : null;
}


Console.WriteLine("Please input the absolute path of the .pak OpenplanetNext\\Extract\\GameData\\Stadium\\GameCtnBlockInfo:");

string blockInfoRootFolder = @"C:\Users\Bux\OpenplanetNext\Extract\GameData\Stadium\GameCtnBlockInfo";



var dict = new Dictionary<string, BlockModel>();


List<TMDojoBlock> blocks = new List<TMDojoBlock>();

string[] ignoredFiles =
{
    "GateSpecialBoost.EDClassic",
    "GateSpecialBoost2.EDClassic",
    "GateSpecialCruise.EDClassic",
    "GateSpecialFragile.EDClassic",
    "GateSpecialNoBrake.EDClassic",
    "GateSpecialNoEngine.EDClassic",
    "GateSpecialNoSteering.EDClassic",
    "GateSpecialReset.EDClassic",
    "GateSpecialSlowMotion.EDClassic",
    "GateSpecialTurbo.EDClassic",
    "GateSpecialTurbo2.EDClassic",
    "GateSpecialTurboRoulette.EDClassic",
    "CanopyBorderBeamCurveInFCLeft.EDClip",
    "CanopyBorderBeamCurveInFCRight.EDClip",
    "CanopyBorderBeamCurveOutFCLeft.EDClip"
};

foreach (var blockInfoFolder in Directory.EnumerateDirectories(blockInfoRootFolder, ".", SearchOption.TopDirectoryOnly))
{
    foreach (var blockInfoFile in Directory.EnumerateFiles(blockInfoFolder, "*.*", SearchOption.TopDirectoryOnly))
    {
        try
        {
            string blockFileName = Path.GetFileNameWithoutExtension(blockInfoFile);

            if (ignoredFiles.Contains(blockFileName))
            {
                Console.WriteLine($">> SKIP {blockFileName}");
                continue;
            }
            else
            {
                Console.WriteLine(blockFileName);
            }
            try
            {
                var xd = GameBox.ParseNode(blockInfoFile);


                if (GameBox.ParseNode(blockInfoFile) is not CGameCtnBlockInfoClassic blockInfo)
                {
                    continue;
                }

                var block = new BlockModel
                {
                    Air = CreateVariant(blockInfo.VariantBaseAir),
                    Ground = CreateVariant(blockInfo.VariantBaseGround)
                };

                List<List<float>> offsetsAir = new List<List<float>>();
                List<List<float>> offsetsGround = new List<List<float>>();

                foreach (var offset in block.Air)
                {
                    offsetsAir.Add(new List<float> { offset.Coord.Z, offset.Coord.Y, offset.Coord.X });
                }

                foreach (var offset in block.Ground)
                {
                    offsetsGround.Add(new List<float> { offset.Coord.Z, offset.Coord.Y, offset.Coord.X });
                }

                TMDojoBlock tmDojoBlock = new TMDojoBlock(
                    blockInfo.Ident.Id,
                    offsetsAir,
                    offsetsGround
                );

                blocks.Add(tmDojoBlock);

                dict.Add(blockInfo.Ident.Id, block);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        catch (NotAGbxException e)
        {
            Console.WriteLine($">>>>>{e.Message}");
        }
    }
}


Console.ReadLine();

File.WriteAllText("Clips.json", JsonSerializer.Serialize(blocks));

public class TMDojoBlock
{
    public string name { get; set; }
    public List<List<float>> blockOffsetsAir { get; set; }
    public List<List<float>> blockOffsetsGround { get; set; }

    public TMDojoBlock(string name, List<List<float>> blockOffsetsAir, List<List<float>> blockOffsetsGround)
    {
        this.name = name;
        this.blockOffsetsAir = blockOffsetsAir;
        this.blockOffsetsGround = blockOffsetsGround;
    }
}