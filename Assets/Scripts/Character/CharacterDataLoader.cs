using System.Collections.Generic;
using UnityEngine;

public class CharacterDataLoader : MonoBehaviour
{
    public TextAsset csvFile; // 인스펙터에서 할당
    public List<CharacterInfo> characterList = new List<CharacterInfo>();

    void Awake()
    {
        LoadCSV();
    }

    void LoadCSV()
    {
        string[] lines = csvFile.text.Split('\n');

        // 첫 번째, 두 번째 줄은 헤더니까 건너뜀
        for (int i = 2; i < lines.Length; i++) // 0,1번줄은 건너뜀
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] data = lines[i].Split(',');

            if (data.Length < 6)
                continue;

            if (!int.TryParse(data[0], out int parsedID))
            {
                Debug.LogWarning($"잘못된 ID 값: {data[0]} (줄 {i + 1})");
                continue;
            }

            CharacterInfo info = new CharacterInfo
            {
                InfoID = parsedID,
                Name = data[1],
                Desc = data[2],
                Attack = float.TryParse(data[3], out var atk) ? atk : 0f,
                AttackRange = float.TryParse(data[4], out var rng) ? rng : 0f,
                ControllerPath = data[5].Trim()
            };

            characterList.Add(info);
        }

    }

    void Start()
    {
        foreach (var character in characterList)
        {
            // 1. 이름이 "Character"인 GameObject 찾기 (하나만 있다고 가정)
            GameObject go = GameObject.Find("Character");

            if (go == null)
            {
                Debug.LogError("Character 오브젝트를 찾을 수 없습니다.");
                return;
            }

            // 2. 하위의 Sprite 오브젝트 찾기
            Transform spriteTransform = go.transform.Find("CharacterSprite");
            if (spriteTransform != null)
            {
                Animator animator = spriteTransform.GetComponent<Animator>();
                if (animator != null)
                {
                    // 애니메이션 경로로부터 AnimationClip 불러오기
                    RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(GetResourcesPath(character.ControllerPath));
                    if (controller != null)
                    {
                        animator.runtimeAnimatorController = controller;
                    }
                }
            }

            // 3. 공격 범위 반영
            Transform attackArea = go.transform.Find("AttackArea");
            if (attackArea != null)
            {
                CircleCollider2D col = attackArea.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = character.AttackRange;
                }
            }

            break; // 테스트용으로 첫 캐릭터만 적용
        }
    }

    // 경로에서 Assets/ 와 .anim 제거 → Resources.Load용 상대경로로
    string GetResourcesPath(string assetPath)
    {
        // "Assets/Resources/" 제거하고 ".controller"도 제거해서 Resources.Load 용 경로로 만듦
        string trimmed = assetPath.Replace("Assets/Resources/", "");
        return trimmed.Replace(".controller", "");
    }

}

