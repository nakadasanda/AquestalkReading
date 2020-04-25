using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;

namespace aquestalk
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        /// 

        const string dllname = "AquesTalk.dll";

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        struct AQTK_VOICE
        {
            public int bas;    // 基本素片 F1E/F2E/M1E (0/1/2)
            public int spd;    // 話速 	50-300 default:100
            public int vol;    // 音量 	0-300 default:100
            public int pit;    // 高さ 	20-200 default:基本素片に依存
            public int acc;    // アクセント 0-200 default:基本素片に依存
            public int lmd;    // 音程１ 	0-200 default:100
            public int fsc;	// 音程２(サンプリング周波数) 50-200 default:100
            public void Init()
            {
                bas = 0;
                spd = 100;
                vol = 100;
                pit = 100;
                acc = 100;
                lmd = 100;
                fsc = 100;
            }
        }

        [DllImport(dllname)]
        extern static IntPtr AquesTalk_Synthe_Utf8(ref AQTK_VOICE pParam, byte[] koe, ref int size);

        [DllImport(dllname)]
        //声
        extern static void AquesTalk_FreeWave(IntPtr wav);
        [STAThread]
        static void Main()
        {


            while (true)
            {
                //不定長情報のメモリー確保
                IntPtr aqtk_p = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(AQTK_VOICE)));
                AQTK_VOICE aqtk_voice = (AQTK_VOICE)Marshal.PtrToStructure(aqtk_p, typeof(AQTK_VOICE));
                aqtk_voice.Init();

                //文字読み込み
                Console.WriteLine("何かひらがなを入れてください。");
                string koe = Console.ReadLine();
                System.Text.Encoding utf8Enc = System.Text.Encoding.GetEncoding("UTF-8");

                byte[] koeUtfBytes = utf8Enc.GetBytes(koe);

                int size = 0;
                IntPtr wavPtr = AquesTalk_Synthe_Utf8(ref aqtk_voice, koeUtfBytes, ref size);

                if (wavPtr == IntPtr.Zero)
                {
                }

                //C#であつかえるように
                byte[] wav_data = new byte[size];
                Marshal.Copy(wavPtr, wav_data, 0, size);

                //解放
                AquesTalk_FreeWave(wavPtr);

                //再生
                using (var ms = new MemoryStream(wav_data))
                using (var sp = new SoundPlayer(ms))
                {
                    sp.Play();
                }
            }

        }
    }
}
