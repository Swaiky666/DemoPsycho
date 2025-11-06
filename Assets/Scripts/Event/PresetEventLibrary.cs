using UnityEngine;

/// <summary>
/// 预设事件库 - 包含 20+ 个完整的事件定义
/// 这些事件可以直接复制到 EventDatabase 中使用
/// 
/// 使用方法：
/// 1. 创建 EventDatabase.asset
/// 2. 从此类复制所需事件
/// 3. 粘贴到 Inspector 中的 Events 数组
/// </summary>
public static class PresetEventLibrary
{
    /// <summary>
    /// 获取所有预设事件
    /// </summary>
    public static EventData[] GetAllPresetEvents()
    {
        return new EventData[]
        {
            // ===== 个人类事件 =====
            GetOverworkEvent(),
            GetDepressionEvent(),
            GetHealthCrisisEvent(),
            GetBurnoutEvent(),
            
            // ===== 财务类事件 =====
            GetWealthEvent(),
            GetPovertyEvent(),
            GetInvestmentOpportunityEvent(),
            GetUnexpensesEvent(),
            
            // ===== 工作类事件 =====
            GetJobOpportunityEvent(),
            GetConflictAtWorkEvent(),
            GetPromotionEvent(),
            GetJobLossEvent(),
            
            // ===== 社交类事件 =====
            GetLonelinessEvent(),
            GetFriendshipEvent(),
            GetConflictWithFriendsEvent(),
            
            // ===== 里程碑事件 =====
            GetFirstWeekCompletedEvent(),
            GetMonthAnniversaryEvent(),
            GetLifeChangeEvent(),
            
            // ===== 特殊事件 =====
            GetWeekendEvent(),
            GetRandomMeetingEvent(),
            GetSerendipityEvent()
        };
    }

    // ===== 个人类事件 =====

    private static EventData GetOverworkEvent()
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
                new TriggerCondition { type = ConditionType.Health, value = 40, comparison = ComparisonType.Less }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "overwork_rest",
                    choiceTextKey = "choice_overwork_rest",
                    effects = new string[] { "health+30", "A-2" },
                    onChoiceFlag = "chose_rest_for_overwork"
                },
                new EventChoice
                {
                    choiceId = "overwork_continue",
                    choiceTextKey = "choice_overwork_continue",
                    effects = new string[] { "health-10", "V-2", "A+2" },
                    onChoiceFlag = "chose_continue_for_overwork"
                }
            },
            
            onTriggerFlag = "overwork_event_occurred"
        };
    }

    private static EventData GetDepressionEvent()
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
                new TriggerCondition { type = ConditionType.Valence, value = -5, comparison = ComparisonType.Less }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "depression_seek_help",
                    choiceTextKey = "choice_depression_seek_help",
                    effects = new string[] { "V+2", "A-1" },
                    onChoiceFlag = "depression_seeking_help"
                },
                new EventChoice
                {
                    choiceId = "depression_endure",
                    choiceTextKey = "choice_depression_endure",
                    effects = new string[] { "V+0.5", "A+0.5" }
                }
            },
            
            onTriggerFlag = "depression_event_occurred"
        };
    }

    private static EventData GetHealthCrisisEvent()
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
                new TriggerCondition { type = ConditionType.Health, value = 20, comparison = ComparisonType.Less }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "health_crisis_seek_doctor",
                    choiceTextKey = "choice_see_doctor",
                    effects = new string[] { "health+40", "gold-100", "V+1" }
                },
                new EventChoice
                {
                    choiceId = "health_crisis_ignore",
                    choiceTextKey = "choice_ignore",
                    effects = new string[] { "health-20", "V-2" }
                }
            },
            
            excludedFlags = new string[] { "health_crisis_occurred_recently" },
            onTriggerFlag = "health_crisis_occurred_recently"
        };
    }

    private static EventData GetBurnoutEvent()
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
                new TriggerCondition { type = ConditionType.Health, value = 50, comparison = ComparisonType.Less },
                new TriggerCondition { type = ConditionType.Valence, value = 2, comparison = ComparisonType.Less }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "burnout_vacation",
                    choiceTextKey = "choice_take_vacation",
                    effects = new string[] { "health+50", "V+3", "A-2", "gold-200" },
                    nextEventId = "event_recovery"
                },
                new EventChoice
                {
                    choiceId = "burnout_push_through",
                    choiceTextKey = "choice_push_through",
                    effects = new string[] { "gold+100", "health-30", "V-3" }
                }
            },
            
            onTriggerFlag = "burnout_event_occurred",
            eventWeight = 1.5f
        };
    }

    // ===== 财务类事件 =====

    private static EventData GetWealthEvent()
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
                new TriggerCondition { type = ConditionType.Gold, value = 5000, comparison = ComparisonType.GreaterOrEqual }
            },
            
            excludedFlags = new string[] { "poor_status" },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "wealth_save",
                    choiceTextKey = "choice_wealth_save",
                    effects = new string[] { "V+1", "A-1" },
                    onChoiceFlag = "wealth_saved"
                },
                new EventChoice
                {
                    choiceId = "wealth_spend",
                    choiceTextKey = "choice_wealth_spend",
                    effects = new string[] { "gold-500", "V+2", "health+20" },
                    onChoiceFlag = "wealth_spent"
                }
            },
            
            onTriggerFlag = "wealth_event_occurred"
        };
    }

    private static EventData GetPovertyEvent()
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
                new TriggerCondition { type = ConditionType.Gold, value = 100, comparison = ComparisonType.Less }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "poverty_work_hard",
                    choiceTextKey = "choice_work_hard",
                    effects = new string[] { "V-1", "A+1", "health-5" },
                    onChoiceFlag = "chose_work_for_poverty"
                },
                new EventChoice
                {
                    choiceId = "poverty_get_help",
                    choiceTextKey = "choice_get_help",
                    effects = new string[] { "gold+300", "V+1", "A-1" }
                }
            },
            
            onTriggerFlag = "poverty_event_occurred"
        };
    }

    private static EventData GetInvestmentOpportunityEvent()
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
                    choiceId = "investment_risky",
                    choiceTextKey = "choice_invest_risky",
                    effects = new string[] { "gold+500", "V+2", "A+1" },
                    onChoiceFlag = "investment_risky_taken"
                },
                new EventChoice
                {
                    choiceId = "investment_safe",
                    choiceTextKey = "choice_invest_safe",
                    effects = new string[] { "gold+100", "V+0.5" }
                }
            }
        };
    }

    private static EventData GetUnexpensesEvent()
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
                    effects = new string[] { "gold-200", "V-1", "A+1" }
                },
                new EventChoice
                {
                    choiceId = "expense_defer",
                    choiceTextKey = "choice_expense_defer",
                    effects = new string[] { "A+2", "V-2" }
                }
            }
        };
    }

    // ===== 工作类事件 =====

    private static EventData GetJobOpportunityEvent()
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
                new TriggerCondition { type = ConditionType.WorkSkill, value = 30, comparison = ComparisonType.GreaterOrEqual }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "job_opp_take",
                    choiceTextKey = "choice_take_opportunity",
                    effects = new string[] { "workSkill+10", "gold+200", "V+1" },
                    onChoiceFlag = "risk_taker"
                },
                new EventChoice
                {
                    choiceId = "job_opp_safe",
                    choiceTextKey = "choice_play_safe",
                    effects = new string[] { "V-0.5", "A+0.5" }
                }
            }
        };
    }

    private static EventData GetConflictAtWorkEvent()
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
                    effects = new string[] { "V+1", "A-1" }
                },
                new EventChoice
                {
                    choiceId = "conflict_ignore",
                    choiceTextKey = "choice_ignore",
                    effects = new string[] { "V-2", "A+2" }
                }
            }
        };
    }

    private static EventData GetPromotionEvent()
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
                new TriggerCondition { type = ConditionType.WorkSkill, value = 50, comparison = ComparisonType.GreaterOrEqual }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "promo_accept",
                    choiceTextKey = "choice_accept_promotion",
                    effects = new string[] { "workSkill+20", "gold+300", "V+2", "A+1" },
                    onChoiceFlag = "got_promoted"
                },
                new EventChoice
                {
                    choiceId = "promo_decline",
                    choiceTextKey = "choice_decline_promotion",
                    effects = new string[] { "V-1", "A+1" }
                }
            }
        };
    }

    private static EventData GetJobLossEvent()
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
                    choiceId = "jobloss_find_new",
                    choiceTextKey = "choice_find_new_job",
                    effects = new string[] { "V-2", "A+2", "health-10" },
                    onChoiceFlag = "job_hunting"
                },
                new EventChoice
                {
                    choiceId = "jobloss_take_break",
                    choiceTextKey = "choice_take_break",
                    effects = new string[] { "health+30", "V+1", "gold-100" }
                }
            }
        };
    }

    // ===== 社交类事件 =====

    private static EventData GetLonelinessEvent()
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
                new TriggerCondition { type = ConditionType.Valence, value = 0, comparison = ComparisonType.LessOrEqual }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "loneliness_reach_out",
                    choiceTextKey = "choice_reach_out",
                    effects = new string[] { "V+2", "A-1" },
                    onChoiceFlag = "reached_out_to_friend"
                },
                new EventChoice
                {
                    choiceId = "loneliness_withdraw",
                    choiceTextKey = "choice_withdraw",
                    effects = new string[] { "V-2", "A-1" }
                }
            }
        };
    }

    private static EventData GetFriendshipEvent()
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
                    onChoiceFlag = "made_friend"
                },
                new EventChoice
                {
                    choiceId = "friend_hesitate",
                    choiceTextKey = "choice_hesitate",
                    effects = new string[] { "V+1", "A+0.5" }
                }
            }
        };
    }

    private static EventData GetConflictWithFriendsEvent()
    {
        return new EventData
        {
            eventId = "event_friend_conflict",
            eventName = "友谊纠纷",
            category = EventCategory.Personal,
            storyKey = "story_friend_conflict",
            triggerProbability = 0.08f,
            
            requiredFlags = new string[] { "made_friend" },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "friend_conflict_reconcile",
                    choiceTextKey = "choice_reconcile",
                    effects = new string[] { "V+1", "emotionStability+3" }
                },
                new EventChoice
                {
                    choiceId = "friend_conflict_separate",
                    choiceTextKey = "choice_separate",
                    effects = new string[] { "V-2", "A+1" }
                }
            }
        };
    }

    // ===== 里程碑事件 =====

    private static EventData GetFirstWeekCompletedEvent()
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
                    choiceId = "first_week_reflect",
                    choiceTextKey = "choice_reflect",
                    effects = new string[] { "V+2", "A-1" },
                    onChoiceFlag = GameFlagManager.FlagNames.FIRST_WEEK_COMPLETED
                }
            },
            
            canSkip = false
        };
    }

    private static EventData GetMonthAnniversaryEvent()
    {
        return new EventData
        {
            eventId = "event_month_anniversary",
            eventName = "一个月纪念",
            category = EventCategory.Special,
            storyKey = "story_month_anniversary",
            triggerProbability = 0f,
            
            requiredFlags = new string[] { GameFlagManager.FlagNames.FIRST_WEEK_COMPLETED },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "anniversary_celebrate",
                    choiceTextKey = "choice_celebrate",
                    effects = new string[] { "V+2", "gold-100", "health+20" },
                    onChoiceFlag = GameFlagManager.FlagNames.FIRST_MONTH_COMPLETED
                }
            }
        };
    }

    private static EventData GetLifeChangeEvent()
    {
        return new EventData
        {
            eventId = "event_life_change",
            eventName = "人生转折",
            category = EventCategory.Special,
            storyKey = "story_life_change",
            triggerProbability = 0.05f,
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "life_change_embrace",
                    choiceTextKey = "choice_embrace_change",
                    effects = new string[] { "V+2", "workSkill+5", "emotionStability+10" }
                },
                new EventChoice
                {
                    choiceId = "life_change_resist",
                    choiceTextKey = "choice_resist_change",
                    effects = new string[] { "V-1", "A+1" }
                }
            },
            
            eventWeight = 0.5f  // 低权重，特殊事件
        };
    }

    // ===== 特殊事件 =====

    private static EventData GetWeekendEvent()
    {
        return new EventData
        {
            eventId = "event_weekend",
            eventName = "周末休闲",
            category = EventCategory.Random,
            storyKey = "story_weekend",
            triggerProbability = 1f,  // 周末必定触发
            
            conditions = new TriggerCondition[]
            {
                new TriggerCondition { type = ConditionType.DayOfWeek, value = 6, comparison = ComparisonType.GreaterOrEqual }
            },
            
            choices = new EventChoice[]
            {
                new EventChoice
                {
                    choiceId = "weekend_relax",
                    choiceTextKey = "choice_relax",
                    effects = new string[] { "health+20", "V+1", "A-1" }
                },
                new EventChoice
                {
                    choiceId = "weekend_work",
                    choiceTextKey = "choice_work_weekend",
                    effects = new string[] { "gold+100", "health-10", "V-1" }
                }
            }
        };
    }

    private static EventData GetRandomMeetingEvent()
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
                    effects = new string[] { "V+1", "A+0.5" }
                },
                new EventChoice
                {
                    choiceId = "meeting_avoid",
                    choiceTextKey = "choice_avoid",
                    effects = new string[] { "V-0.5", "A+1" }
                }
            }
        };
    }

    private static EventData GetSerendipityEvent()
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
                    choiceId = "serendipity_accept",
                    choiceTextKey = "choice_accept_luck",
                    effects = new string[] { "gold+250", "V+2" },
                    onChoiceFlag = "received_luck"
                }
            },
            
            canSkip = true,
            eventWeight = 0.8f
        };
    }
}

// 使用方法：
// 1. 创建 EventDatabase.asset
// 2. 运行此脚本中的 GetAllPresetEvents()
// 3. 将返回的数组设置到 EventDatabase
//
// 或在代码中：
// var allEvents = PresetEventLibrary.GetAllPresetEvents();
// eventDatabase.events = allEvents;