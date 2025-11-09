using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Event Database 配置编辑器
/// 自动填充预设事件到 EventDatabase
/// 
/// 使用方法:
/// 1. 菜单 → Tools → Events → Setup Event Database
/// 2. 选择或创建 EventDatabase.asset
/// 3. 自动填充 20+ 个预设事件
/// </summary>
public class EventDatabaseSetupEditor
{
    [MenuItem("Tools/Events/Setup Event Database")]
    public static void SetupEventDatabase()
    {
        // 1. 查找或创建 EventDatabase
        EventDatabase database = FindOrCreateEventDatabase();
        
        if (database == null)
        {
            EditorUtility.DisplayDialog("错误", "无法创建或找到 EventDatabase", "确定");
            return;
        }

        // 2. 生成所有预设事件
        List<EventData> events = GenerateAllEvents();
        
        // 3. 写入到 database
        database.events = events.ToArray();
        database.Initialize();

        // 4. 保存
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "成功", 
            $"已添加 {events.Count} 个事件到 EventDatabase\n\n保存位置: {AssetDatabase.GetAssetPath(database)}", 
            "确定"
        );

        Debug.Log($"[EventDatabaseSetup] 成功添加 {events.Count} 个事件");
    }

    /// <summary>
    /// 查找或创建 EventDatabase
    /// </summary>
    private static EventDatabase FindOrCreateEventDatabase()
    {
        // 尝试从 Resources 加载
        EventDatabase database = Resources.Load<EventDatabase>("Events/EventDatabase");
        
        if (database != null)
        {
            Debug.Log("[EventDatabaseSetup] 找到现有 EventDatabase");
            return database;
        }

        // 尝试从项目中查找
        string[] guids = AssetDatabase.FindAssets("t:EventDatabase");
        
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            database = AssetDatabase.LoadAssetAtPath<EventDatabase>(path);
            Debug.Log($"[EventDatabaseSetup] 找到现有 EventDatabase: {path}");
            return database;
        }

        // 创建新的 EventDatabase
        database = ScriptableObject.CreateInstance<EventDatabase>();
        
        // 确保文件夹存在
        string folderPath = "Assets/Resources/Events";
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(folderPath))
            AssetDatabase.CreateFolder("Assets/Resources", "Events");
        
        string assetPath = $"{folderPath}/EventDatabase.asset";
        AssetDatabase.CreateAsset(database, assetPath);
        
        Debug.Log($"[EventDatabaseSetup] 创建新 EventDatabase: {assetPath}");
        return database;
    }

    /// <summary>
    /// 生成所有预设事件
    /// </summary>
    private static List<EventData> GenerateAllEvents()
    {
        var events = new List<EventData>();

        // ===== 饥饿相关事件 (新增) =====
        events.Add(CreateHungerCrisisEvent());
        events.Add(CreateSkipMealEvent());
        events.Add(CreateFoodShortageEvent());

        // ===== 个人类事件 =====
        events.Add(CreateOverworkEvent());
        events.Add(CreateDepressionEvent());
        events.Add(CreateHealthCrisisEvent());
        events.Add(CreateBurnoutEvent());
        
        // ===== 财务类事件 =====
        events.Add(CreateWealthEvent());
        events.Add(CreatePovertyEvent());
        events.Add(CreateInvestmentOpportunityEvent());
        events.Add(CreateUnexpectedExpenseEvent());
        
        // ===== 工作类事件 =====
        events.Add(CreateJobOpportunityEvent());
        events.Add(CreateWorkConflictEvent());
        events.Add(CreatePromotionEvent());
        events.Add(CreateJobLossEvent());
        
        // ===== 社交类事件 =====
        events.Add(CreateLonelinessEvent());
        events.Add(CreateFriendshipEvent());
        
        // ===== 里程碑事件 =====
        events.Add(CreateFirstWeekEvent());
        events.Add(CreateMonthAnniversaryEvent());
        
        // ===== 随机事件 =====
        events.Add(CreateWeekendEvent());
        events.Add(CreateRandomMeetingEvent());
        events.Add(CreateSerendipityEvent());

        return events;
    }

    // ========== 饥饿相关事件 (新增) ==========

    private static EventData CreateHungerCrisisEvent()
    {
        return new EventData
        {
            eventId = "event_hunger_crisis",
            eventName = "饥饿难耐",
            category = EventCategory.Personal,
            storyKey = "story_hunger_crisis",
            triggerProbability = 0f,  // 条件触发
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Health,  // 使用 Health 作为饥饿值的占位符
                    value = 20, 
                    comparison = ComparisonType.Less 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "hunger_buy_food",
                    choiceTextKey = "choice_hunger_buy_food",
                    effects = new string[] { "gold-30", "hunger+50", "V+2" },
                    resultTextKey = "result_hunger_buy_food"
                },
                new EventChoice
                {
                    choiceId = "hunger_endure",
                    choiceTextKey = "choice_hunger_endure",
                    effects = new string[] { "V-3", "A+2", "health-10" },
                    resultTextKey = "result_hunger_endure"
                }
            },
            
            onTriggerFlag = "hunger_crisis_occurred"
        };
    }

    private static EventData CreateSkipMealEvent()
    {
        return new EventData
        {
            eventId = "event_skip_meal",
            eventName = "错过用餐时间",
            category = EventCategory.Random,
            storyKey = "story_skip_meal",
            triggerProbability = 0.15f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.TimeOfDay, 
                    value = 1,  // 中午
                    comparison = ComparisonType.Equal 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "skip_quick_bite",
                    choiceTextKey = "choice_skip_quick_bite",
                    effects = new string[] { "gold-10", "hunger+20", "time-0.5" },
                    resultTextKey = "result_skip_quick_bite"
                },
                new EventChoice
                {
                    choiceId = "skip_continue_work",
                    choiceTextKey = "choice_skip_continue_work",
                    effects = new string[] { "V-1", "A+1" },
                    resultTextKey = "result_skip_continue_work"
                }
            }
        };
    }

    private static EventData CreateFoodShortageEvent()
    {
        return new EventData
        {
            eventId = "event_food_shortage",
            eventName = "囊中羞涩",
            category = EventCategory.Random,
            storyKey = "story_food_shortage",
            triggerProbability = 0.1f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Gold, 
                    value = 50, 
                    comparison = ComparisonType.Less 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "food_cheap_meal",
                    choiceTextKey = "choice_food_cheap_meal",
                    effects = new string[] { "gold-5", "hunger+15", "V-0.5" },
                    resultTextKey = "result_food_cheap_meal"
                },
                new EventChoice
                {
                    choiceId = "food_borrow_money",
                    choiceTextKey = "choice_food_borrow_money",
                    effects = new string[] { "gold+50", "V-1", "A+1" },
                    resultTextKey = "result_food_borrow_money",
                    onChoiceFlag = "borrowed_money_for_food"
                }
            }
        };
    }

    // ========== 个人类事件 ==========

    private static EventData CreateOverworkEvent()
    {
        return new EventData
        {
            eventId = "event_overwork",
            eventName = "过劳症状",
            category = EventCategory.Personal,
            storyKey = "story_overwork",
            triggerProbability = 0f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Health, 
                    value = 40, 
                    comparison = ComparisonType.Less 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "overwork_rest",
                    choiceTextKey = "choice_overwork_rest",
                    effects = new string[] { "health+30", "A-2", "hunger-5" },
                    resultTextKey = "result_overwork_rest",
                    onChoiceFlag = "chose_rest_for_overwork"
                },
                new EventChoice
                {
                    choiceId = "overwork_continue",
                    choiceTextKey = "choice_overwork_continue",
                    effects = new string[] { "health-10", "V-2", "A+2" },
                    resultTextKey = "result_overwork_continue",
                    onChoiceFlag = "chose_continue_for_overwork"
                }
            },
            
            onTriggerFlag = "overwork_event_occurred"
        };
    }

    private static EventData CreateDepressionEvent()
    {
        return new EventData
        {
            eventId = "event_depression",
            eventName = "情绪低谷",
            category = EventCategory.Personal,
            storyKey = "story_depression",
            triggerProbability = 0f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Valence, 
                    value = -5, 
                    comparison = ComparisonType.Less 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "depression_seek_help",
                    choiceTextKey = "choice_depression_seek_help",
                    effects = new string[] { "V+2", "A-1" },
                    resultTextKey = "result_depression_seek_help",
                    onChoiceFlag = "depression_seeking_help"
                },
                new EventChoice
                {
                    choiceId = "depression_endure",
                    choiceTextKey = "choice_depression_endure",
                    effects = new string[] { "V+0.5", "A+0.5" },
                    resultTextKey = "result_depression_endure"
                }
            },
            
            onTriggerFlag = "depression_event_occurred"
        };
    }

    private static EventData CreateHealthCrisisEvent()
    {
        return new EventData
        {
            eventId = "event_health_crisis",
            eventName = "健康危机",
            category = EventCategory.Personal,
            storyKey = "story_health_crisis",
            triggerProbability = 0.05f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Health, 
                    value = 20, 
                    comparison = ComparisonType.Less 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "health_see_doctor",
                    choiceTextKey = "choice_see_doctor",
                    effects = new string[] { "health+40", "gold-100", "V+1" },
                    resultTextKey = "result_see_doctor"
                },
                new EventChoice
                {
                    choiceId = "health_ignore",
                    choiceTextKey = "choice_ignore",
                    effects = new string[] { "health-20", "V-2" },
                    resultTextKey = "result_ignore"
                }
            },
            
            excludedFlags = new string[] { "health_crisis_occurred_recently" },
            onTriggerFlag = "health_crisis_occurred_recently"
        };
    }

    private static EventData CreateBurnoutEvent()
    {
        return new EventData
        {
            eventId = "event_burnout",
            eventName = "工作倦怠",
            category = EventCategory.Personal,
            storyKey = "story_burnout",
            triggerProbability = 0.08f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Health, 
                    value = 50, 
                    comparison = ComparisonType.Less 
                },
                new TriggerCondition 
                { 
                    type = ConditionType.Valence, 
                    value = 2, 
                    comparison = ComparisonType.Less 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "burnout_vacation",
                    choiceTextKey = "choice_take_vacation",
                    effects = new string[] { "health+50", "V+3", "A-2", "gold-200" },
                    resultTextKey = "result_take_vacation"
                },
                new EventChoice
                {
                    choiceId = "burnout_push",
                    choiceTextKey = "choice_push_through_burnout",
                    effects = new string[] { "gold+100", "health-30", "V-3" },
                    resultTextKey = "result_push_through_burnout"
                }
            },
            
            onTriggerFlag = "burnout_event_occurred"
        };
    }

    // ========== 财务类事件 ==========

    private static EventData CreateWealthEvent()
    {
        return new EventData
        {
            eventId = "event_wealth",
            eventName = "财富膨胀",
            category = EventCategory.Random,
            storyKey = "story_wealth",
            triggerProbability = 0.15f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Gold, 
                    value = 5000, 
                    comparison = ComparisonType.GreaterOrEqual 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "wealth_save",
                    choiceTextKey = "choice_wealth_save",
                    effects = new string[] { "V+1", "A-1" },
                    resultTextKey = "result_wealth_save",
                    onChoiceFlag = "wealth_saved"
                },
                new EventChoice
                {
                    choiceId = "wealth_spend",
                    choiceTextKey = "choice_wealth_spend",
                    effects = new string[] { "gold-500", "V+2", "health+20", "hunger+30" },
                    resultTextKey = "result_wealth_spend",
                    onChoiceFlag = "wealth_spent"
                }
            },
            
            onTriggerFlag = "wealth_event_occurred"
        };
    }

    private static EventData CreatePovertyEvent()
    {
        return new EventData
        {
            eventId = "event_poverty",
            eventName = "经济困难",
            category = EventCategory.Random,
            storyKey = "story_poverty",
            triggerProbability = 0.1f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Gold, 
                    value = 100, 
                    comparison = ComparisonType.Less 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "poverty_work",
                    choiceTextKey = "choice_work_hard_poverty",
                    effects = new string[] { "V-1", "A+1", "health-5" },
                    resultTextKey = "result_work_hard_poverty",
                    onChoiceFlag = "chose_work_for_poverty"
                },
                new EventChoice
                {
                    choiceId = "poverty_help",
                    choiceTextKey = "choice_get_help_poverty",
                    effects = new string[] { "gold+300", "V+1", "A-1" },
                    resultTextKey = "result_get_help_poverty"
                }
            },
            
            onTriggerFlag = "poverty_event_occurred"
        };
    }

    private static EventData CreateInvestmentOpportunityEvent()
    {
        return new EventData
        {
            eventId = "event_investment",
            eventName = "投资机会",
            category = EventCategory.Random,
            storyKey = "story_investment",
            triggerProbability = 0.12f,
            
            requiredFlags = new string[] { "wealth_saved" },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "invest_risky",
                    choiceTextKey = "choice_invest_risky",
                    effects = new string[] { "gold+500", "V+2", "A+1" },
                    resultTextKey = "result_invest_risky",
                    onChoiceFlag = "investment_risky_taken"
                },
                new EventChoice
                {
                    choiceId = "invest_safe",
                    choiceTextKey = "choice_invest_safe",
                    effects = new string[] { "gold+100", "V+0.5" },
                    resultTextKey = "result_invest_safe"
                }
            }
        };
    }

    private static EventData CreateUnexpectedExpenseEvent()
    {
        return new EventData
        {
            eventId = "event_unexpected_expense",
            eventName = "意外开支",
            category = EventCategory.Random,
            storyKey = "story_unexpected_expense",
            triggerProbability = 0.1f,
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "expense_pay",
                    choiceTextKey = "choice_expense_pay",
                    effects = new string[] { "gold-200", "V-1", "A+1" },
                    resultTextKey = "result_expense_pay"
                },
                new EventChoice
                {
                    choiceId = "expense_defer",
                    choiceTextKey = "choice_expense_defer",
                    effects = new string[] { "A+2", "V-2" },
                    resultTextKey = "result_expense_defer"
                }
            }
        };
    }

    // ========== 工作类事件 ==========

    private static EventData CreateJobOpportunityEvent()
    {
        return new EventData
        {
            eventId = "event_job_opportunity",
            eventName = "职业机遇",
            category = EventCategory.Work,
            storyKey = "story_job_opportunity",
            triggerProbability = 0.12f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.WorkSkill, 
                    value = 30, 
                    comparison = ComparisonType.GreaterOrEqual 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "job_take",
                    choiceTextKey = "choice_take_chance",
                    effects = new string[] { "workSkill+10", "gold+200", "V+1" },
                    resultTextKey = "result_take_chance",
                    onChoiceFlag = "risk_taker"
                },
                new EventChoice
                {
                    choiceId = "job_safe",
                    choiceTextKey = "choice_play_safe",
                    effects = new string[] { "V-0.5", "A+0.5" },
                    resultTextKey = "result_play_safe"
                }
            }
        };
    }

    private static EventData CreateWorkConflictEvent()
    {
        return new EventData
        {
            eventId = "event_work_conflict",
            eventName = "工作冲突",
            category = EventCategory.Work,
            storyKey = "story_work_conflict",
            triggerProbability = 0.1f,
            
            requiredFlags = new string[] { "worked_at_bar" },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "conflict_resolve",
                    choiceTextKey = "choice_resolve",
                    effects = new string[] { "V+1", "A-1" },
                    resultTextKey = "result_resolve"
                },
                new EventChoice
                {
                    choiceId = "conflict_ignore",
                    choiceTextKey = "choice_conflict_ignore",
                    effects = new string[] { "V-2", "A+2" },
                    resultTextKey = "result_conflict_ignore"
                }
            }
        };
    }

    private static EventData CreatePromotionEvent()
    {
        return new EventData
        {
            eventId = "event_promotion",
            eventName = "工作升迁",
            category = EventCategory.Work,
            storyKey = "story_promotion",
            triggerProbability = 0.08f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.WorkSkill, 
                    value = 50, 
                    comparison = ComparisonType.GreaterOrEqual 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "promo_accept",
                    choiceTextKey = "choice_accept_promotion",
                    effects = new string[] { "workSkill+20", "gold+300", "V+2", "A+1" },
                    resultTextKey = "result_accept_promotion",
                    onChoiceFlag = "got_promoted"
                },
                new EventChoice
                {
                    choiceId = "promo_decline",
                    choiceTextKey = "choice_decline_promotion",
                    effects = new string[] { "V-1", "A+1" },
                    resultTextKey = "result_decline_promotion"
                }
            }
        };
    }

    private static EventData CreateJobLossEvent()
    {
        return new EventData
        {
            eventId = "event_job_loss",
            eventName = "失业危机",
            category = EventCategory.Work,
            storyKey = "story_job_loss",
            triggerProbability = 0.05f,
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "jobloss_find",
                    choiceTextKey = "choice_find_new_job",
                    effects = new string[] { "V-2", "A+2", "health-10" },
                    resultTextKey = "result_find_new_job",
                    onChoiceFlag = "job_hunting"
                },
                new EventChoice
                {
                    choiceId = "jobloss_break",
                    choiceTextKey = "choice_take_break_job",
                    effects = new string[] { "health+30", "V+1", "gold-100" },
                    resultTextKey = "result_take_break_job"
                }
            }
        };
    }

    // ========== 社交类事件 ==========

    private static EventData CreateLonelinessEvent()
    {
        return new EventData
        {
            eventId = "event_loneliness",
            eventName = "孤独感",
            category = EventCategory.Personal,
            storyKey = "story_loneliness",
            triggerProbability = 0.1f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.Valence, 
                    value = 0, 
                    comparison = ComparisonType.LessOrEqual 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "lonely_reach",
                    choiceTextKey = "choice_reach_out",
                    effects = new string[] { "V+2", "A-1" },
                    resultTextKey = "result_reach_out",
                    onChoiceFlag = "reached_out_to_friend"
                },
                new EventChoice
                {
                    choiceId = "lonely_withdraw",
                    choiceTextKey = "choice_withdraw",
                    effects = new string[] { "V-2", "A-1" },
                    resultTextKey = "result_withdraw"
                }
            }
        };
    }

    private static EventData CreateFriendshipEvent()
    {
        return new EventData
        {
            eventId = "event_friendship",
            eventName = "新的友谊",
            category = EventCategory.Personal,
            storyKey = "story_friendship",
            triggerProbability = 0.12f,
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "friend_embrace",
                    choiceTextKey = "choice_embrace",
                    effects = new string[] { "V+3", "A+1", "emotionStability+5" },
                    resultTextKey = "result_embrace",
                    onChoiceFlag = "made_friend"
                },
                new EventChoice
                {
                    choiceId = "friend_hesitate",
                    choiceTextKey = "choice_hesitate",
                    effects = new string[] { "V+1", "A+0.5" },
                    resultTextKey = "result_hesitate"
                }
            }
        };
    }

    // ========== 里程碑事件 ==========

    private static EventData CreateFirstWeekEvent()
    {
        return new EventData
        {
            eventId = "event_first_week",
            eventName = "第一周完成",
            category = EventCategory.Special,
            storyKey = "story_first_week",
            triggerProbability = 0f,
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "week_reflect",
                    choiceTextKey = "choice_reflect",
                    effects = new string[] { "V+2", "A-1" },
                    resultTextKey = "result_reflect",
                    onChoiceFlag = "first_week_completed"
                }
            },
            
            canSkip = false
        };
    }

    private static EventData CreateMonthAnniversaryEvent()
    {
        return new EventData
        {
            eventId = "event_month_anniversary",
            eventName = "一个月纪念",
            category = EventCategory.Special,
            storyKey = "story_month_anniversary",
            triggerProbability = 0f,
            
            requiredFlags = new string[] { "first_week_completed" },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "month_celebrate",
                    choiceTextKey = "choice_celebrate",
                    effects = new string[] { "V+2", "gold-100", "health+20", "hunger+40" },
                    resultTextKey = "result_celebrate",
                    onChoiceFlag = "first_month_completed"
                }
            }
        };
    }

    // ========== 随机事件 ==========

    private static EventData CreateWeekendEvent()
    {
        return new EventData
        {
            eventId = "event_weekend",
            eventName = "周末休闲",
            category = EventCategory.Random,
            storyKey = "story_weekend",
            triggerProbability = 1f,
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition 
                { 
                    type = ConditionType.DayOfWeek, 
                    value = 6, 
                    comparison = ComparisonType.GreaterOrEqual 
                }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "weekend_relax",
                    choiceTextKey = "choice_relax",
                    effects = new string[] { "health+20", "V+1", "A-1", "hunger-5" },
                    resultTextKey = "result_relax"
                },
                new EventChoice
                {
                    choiceId = "weekend_work",
                    choiceTextKey = "choice_work_weekend",
                    effects = new string[] { "gold+100", "health-10", "V-1", "hunger-10" },
                    resultTextKey = "result_work_weekend"
                }
            }
        };
    }

    private static EventData CreateRandomMeetingEvent()
    {
        return new EventData
        {
            eventId = "event_random_meeting",
            eventName = "巧遇某人",
            category = EventCategory.Random,
            storyKey = "story_random_meeting",
            triggerProbability = 0.15f,
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "meeting_talk",
                    choiceTextKey = "choice_talk",
                    effects = new string[] { "V+1", "A+0.5", "hunger-3" },
                    resultTextKey = "result_talk"
                },
                new EventChoice
                {
                    choiceId = "meeting_avoid",
                    choiceTextKey = "choice_avoid",
                    effects = new string[] { "V-0.5", "A+1" },
                    resultTextKey = "result_avoid"
                }
            }
        };
    }

    private static EventData CreateSerendipityEvent()
    {
        return new EventData
        {
            eventId = "event_serendipity",
            eventName = "幸运降临",
            category = EventCategory.Random,
            storyKey = "story_serendipity",
            triggerProbability = 0.08f,
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "luck_accept",
                    choiceTextKey = "choice_accept_luck",
                    effects = new string[] { "gold+250", "V+2" },
                    resultTextKey = "result_accept_luck",
                    onChoiceFlag = "received_luck"
                }
            },
            
            canSkip = true,
            eventWeight = 0.8f
        };
    }

    // ========== 验证和调试方法 ==========

    [MenuItem("Tools/Events/Validate Event Database")]
    public static void ValidateEventDatabase()
    {
        EventDatabase database = FindOrCreateEventDatabase();
        
        if (database == null || database.events.Length == 0)
        {
            EditorUtility.DisplayDialog("验证失败", "EventDatabase 为空或不存在", "确定");
            return;
        }

        int errors = 0;
        string errorLog = "";

        foreach (var evt in database.events)
        {
            // 检查基本信息
            if (string.IsNullOrEmpty(evt.eventId))
            {
                errorLog += $"• 事件 ID 为空\n";
                errors++;
            }

            // 检查选择项
            if (evt.choices == null || evt.choices.Length == 0)
            {
                errorLog += $"• {evt.eventName}: 没有选择项\n";
                errors++;
            }
        }

        string message = errors == 0 
            ? $"✓ 验证通过\n共 {database.events.Length} 个事件" 
            : $"发现 {errors} 个错误:\n\n{errorLog}";

        EditorUtility.DisplayDialog("验证结果", message, "确定");
        Debug.Log($"[EventDatabase Validation] {message}");
    }

    [MenuItem("Tools/Events/Print Event Database Info")]
    public static void PrintEventDatabaseInfo()
    {
        EventDatabase database = FindOrCreateEventDatabase();
        
        if (database == null)
        {
            Debug.LogError("[EventDatabase] 未找到 EventDatabase");
            return;
        }

        Debug.Log($"\n========== Event Database 信息 ==========");
        Debug.Log($"总事件数: {database.events.Length}");
        
        var categories = new Dictionary<EventCategory, int>();
        
        foreach (var evt in database.events)
        {
            if (!categories.ContainsKey(evt.category))
                categories[evt.category] = 0;
            categories[evt.category]++;
        }

        Debug.Log("\n按分类统计:");
        foreach (var kvp in categories)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value} 个");
        }

        Debug.Log($"=========================================\n");
    }
}