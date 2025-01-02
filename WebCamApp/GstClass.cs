using System;
using System.Security.Principal;
using System.Xml.Linq;
using GLib;
using Gst;
using Gst.Video;

public class GstClass {
	private Gst.Pipeline? Pipeline { get; set; }
	private uint FrameCount = 0;
	private uint NumBuffers = 0;
	private bool Init = false;
	// callback用delegater
	delegate void MyDelegater(object sender, GLib.SignalArgs args);
	public Delegate? Delegater = null;
	private string IdentName = "IDT";
	private bool Finished = false;

	public GstClass() {
		Delegater = new MyDelegater(MyHandoffCallback);
	}

	// 最初に実行する
	public void Initialize() {
		if (Init) return;
		Init = true;

		Gst.Application.Init();
		GtkSharp.GstreamerSharp.ObjectManager.Initialize();
	}

	public bool IsFinished() { return Finished; }
	public void Clear() { Finished = false; }

	private void BusSyncMessage(object o, SyncMessageArgs args) {
		string msg;

		switch (args.Message.Type) {
			case MessageType.Error:
				GLib.GException err;
				args.Message.ParseError(out err, out msg);
				break;
			case MessageType.Eos:
				break;
			case MessageType.NewClock:
				break;
			default:
				break;
		}
	}

	private void BusMessage(object o, MessageArgs args) {
		//Console.WriteLine(args);
	}

	// 停止
	public void Stop() {
		if (Pipeline == null) return;
		SendEOS();

		if (!RemoveCallback(IdentName, "handoff", Delegater)) {
			Console.WriteLine("UnsetCallback fail.");
		}

		Pipeline.Bus.SyncMessage -= BusSyncMessage;
		Pipeline.Bus.Message -= BusMessage;
		Pipeline.SetState(State.Null);
		Pipeline.Dispose();
		Pipeline = null!;
	}

	// 開始
	public bool Start(string src , uint sec, uint fps) {
		Clear();
		if (string.IsNullOrEmpty(src)) return false;

		if (Pipeline != null) {
			Pipeline.SetState(State.Null);
		}

		// パイプライン生成
		try {
			Pipeline = (Pipeline)Parse.Launch(src);
		}
		catch {
			Console.WriteLine("Parse launch error.");
			return false;
		}

		Pipeline.Bus.EnableSyncMessageEmission();
		Pipeline.Bus.AddSignalWatch();
		Pipeline.Bus.SyncMessage += BusSyncMessage;
		Pipeline.Bus.Message += BusMessage;

		NumBuffers = fps * sec; // fps * 秒数でフレーム数を計算
		FrameCount = 0;

		if (!SetCallback(IdentName, "handoff", Delegater)) {
			Console.WriteLine("SetCallback fail.");
			Stop();
			return false;
		}

		// 録画開始
		var ret = Pipeline.SetState(State.Playing);
		if (ret == StateChangeReturn.Failure) {
			Console.WriteLine("Unable to set the pipeline to the playing state.");
			Stop();
			return false;
		}
		else if (ret == StateChangeReturn.NoPreroll) {
			Console.WriteLine("Playing a live stream.");
		}

		return true;
	}

	public bool Run() { 
		if (Pipeline == null) return false;
		Pipeline.SetState(State.Playing);

		return true;
	}

	public bool SetCallback(string element, string signal, Delegate handler) {
		if (Pipeline == null) return false;
		if (handler == null) return false;

		var elm = Pipeline.GetByName(element);
		if (elm == null) return false;

		elm.Connect(signal, handler);

		return true;
	}

	public bool RemoveCallback(string element, string signal, Delegate handler) {
		if (Pipeline == null) return false;
		if (handler == null) return false;

		var elm = Pipeline.GetByName(element);
		if (elm == null) return false;

		elm.Disconnect(signal, handler);

		return true;
	}

	// identityでフレーム数をカウントするためのコールバック
	private void MyHandoffCallback(object sender, GLib.SignalArgs args) {
		FrameCount += 1;
		Console.WriteLine("FrameCount = " + FrameCount);
		if (FrameCount == NumBuffers) {
			Finished = true;
		}
	}

	// パイプラインを閉じる
	public void SendEOS() {
		if (Pipeline == null) return;
		Pipeline.PostMessage(Gst.Message.NewEos(Pipeline));
		Pipeline.Bus.TimedPopFiltered(Gst.Constants.CLOCK_TIME_NONE, MessageType.Eos | MessageType.Error);
	}
}
