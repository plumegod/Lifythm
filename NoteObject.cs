using System;
using System.Collections;
using System.Collections.Generic;
using SonicBloom.Koreo;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
		[Tooltip("NoteObject的图片")]
		// 用于表示该音符的视觉元素。
		public SpriteRenderer visuals;

		// 如果启用，则为该Note Object包装的KoreographyEvent。 包含相关的时间信息。
		KoreographyEvent trackedEvent;

		// 如果启用，则为包含此Note Object的Lane Controller。
		LaneController laneController;

		// 如果启用，则为控制该Note Object所在游戏的audio game controller。
		AudioGameController gameController;

		private Animator _animator;

		private string noteLineName;

		[SerializeField] private string colHitName = null;
		
		public string NoteLineName => noteLineName;
		
		[SerializeField] private GameObject scoreManagerObj;
		
		#region Methods

		// 为使用准备Note Object。
		public void Initialize(KoreographyEvent evt, Sprite sprite, LaneController laneCont, AudioGameController gameCont,string noteLineName)
		{
			trackedEvent = evt;
			gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
			laneController = laneCont;
			gameController = gameCont;
			this.noteLineName = noteLineName;
			_animator = GetComponent<Animator>();
			GetComponent<BoxCollider2D>().size = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
			scoreManagerObj = GameObject.Find("ScoreManager");

			UpdatePosition();
		}

		// 将Note Object重置为其默认状态。
		void Reset()
		{
			trackedEvent = null;
			laneController = null;
			gameController = null;
			colHitName = "null";
		}

		void Update()
		{
			UpdatePosition();

			//todo: 删除音符重置对象
			if (transform.position.x <= laneController.DeSpawnX)
			{
				scoreManagerObj.SendMessage("OnMiss");
				gameController.ReturnNoteObjectToPool(this);
				Reset();
			}
		}

		//  根据当前音频位置更新音符对象沿着音符轨道的位置。
		void UpdatePosition()
		{
			// 给定单位每秒的当前速度，获取我们穿越的样本数。
			float samplesPerUnit = gameController.SampleRate / gameController.noteSpeed;

			// 我们的位置是根据目标与世界坐标的距离偏移的。这取决于在样本中与“完美时间”距离的距离（Koreography事件的时间！）。
			Vector3 pos = laneController.TargetPosition;
			pos.x -= ((gameController.DelayedSampleTime - trackedEvent.StartSample) / samplesPerUnit) * 2;
			transform.position = pos;
		}

		// 将该音符对象返回到由节奏游戏控制器控制的池中。这有助于减少运行时分配。
		void ReturnToPool()
		{
			gameController.ReturnNoteObjectToPool(this);
			Reset();
		}

		// 当击中音符对象时执行操作。
		public void OnHit(string lineName)
		{
			if(lineName != noteLineName)
				return;

			switch (colHitName)
			{
				case "HitNick":
					scoreManagerObj.SendMessage("OnPerfectHit");
					break;
				case "HitGood":
					scoreManagerObj.SendMessage("OnGoodHit");
					break;
				default:
					return;
			}
			
			_animator.Play("Play");
		}


		private void OnTriggerEnter2D(Collider2D other)
		{
			if(other.gameObject.CompareTag("HitNick") || other.gameObject.CompareTag("HitGood"))
			{
				colHitName = other.gameObject.tag;
			}
		}

		private void OnTriggerExit2D(Collider2D  other)
		{
			if (other.gameObject.CompareTag("HitNick"))
			{
				colHitName = "HitGood";
			}else if (other.gameObject.CompareTag("HitGood"))
			{
				colHitName = "null";
			}
		}

		// 当清除音符对象时执行操作。
		public void OnClear()
		{
			ReturnToPool();
		}

		#endregion
}
