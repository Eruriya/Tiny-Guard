using System.Collections.Generic;
using UnityEngine;

public class CharacterDataLoader : MonoBehaviour
{
    public TextAsset csvFile; // �ν����Ϳ��� �Ҵ�
    public List<CharacterInfo> characterList = new List<CharacterInfo>();

    void Awake()
    {
        LoadCSV();
    }

    void LoadCSV()
    {
        string[] lines = csvFile.text.Split('\n');

        // ù ��°, �� ��° ���� ����ϱ� �ǳʶ�
        for (int i = 2; i < lines.Length; i++) // 0,1������ �ǳʶ�
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] data = lines[i].Split(',');

            if (data.Length < 6)
                continue;

            if (!int.TryParse(data[0], out int parsedID))
            {
                Debug.LogWarning($"�߸��� ID ��: {data[0]} (�� {i + 1})");
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
            // 1. �̸��� "Character"�� GameObject ã�� (�ϳ��� �ִٰ� ����)
            GameObject go = GameObject.Find("Character");

            if (go == null)
            {
                Debug.LogError("Character ������Ʈ�� ã�� �� �����ϴ�.");
                return;
            }

            // 2. ������ Sprite ������Ʈ ã��
            Transform spriteTransform = go.transform.Find("CharacterSprite");
            if (spriteTransform != null)
            {
                Animator animator = spriteTransform.GetComponent<Animator>();
                if (animator != null)
                {
                    // �ִϸ��̼� ��ηκ��� AnimationClip �ҷ�����
                    RuntimeAnimatorController controller = Resources.Load<RuntimeAnimatorController>(GetResourcesPath(character.ControllerPath));
                    if (controller != null)
                    {
                        animator.runtimeAnimatorController = controller;
                    }
                }
            }

            // 3. ���� ���� �ݿ�
            Transform attackArea = go.transform.Find("AttackArea");
            if (attackArea != null)
            {
                CircleCollider2D col = attackArea.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = character.AttackRange;
                }
            }

            break; // �׽�Ʈ������ ù ĳ���͸� ����
        }
    }

    // ��ο��� Assets/ �� .anim ���� �� Resources.Load�� ����η�
    string GetResourcesPath(string assetPath)
    {
        // "Assets/Resources/" �����ϰ� ".controller"�� �����ؼ� Resources.Load �� ��η� ����
        string trimmed = assetPath.Replace("Assets/Resources/", "");
        return trimmed.Replace(".controller", "");
    }

}

