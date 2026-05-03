EXTERNAL add_clue(id)
EXTERNAL start_debate_round(id)
EXTERNAL open_book(next_knot_name)
EXTERNAL start_fail_vignette()

=== base_enter ===
MAI: 咕！這裡應該就是假利亞的基地了，我們先把證據集齊，把他的偷竊行為揭穿吧！
-> END

// --- 電腦 ---
=== clue_pc ===
~ add_clue("clue_pc")
-> DONE

// --- 影印機 ---
=== clue_copy_machine ===
~ add_clue("clue_copy_machine")
-> DONE

// --- 畫布 ---
=== clue_canvas ===
~ add_clue("clue_canvas")
-> DONE

=== talk_blacklia_before_clue ===
黑色利亞_idle: ……
: （黑色利亞連正眼都不想看你，忙著數自己的鈔票）
: （看來還需要更多證據。）    
    -> END

=== talk_blacklia_after_clue ===
黑色利亞_idle: ……
: （黑色利亞連正眼都不想看你，忙著數自己的鈔票）

+ [請停止你的盜圖行為，假利亞！]
    主角: 請停止你的盜圖行為，假利亞！
    黑色利亞_idle: …蛤？盜圖？
    : （黑色利亞數鈔票的手停下來了）
    黑色利亞_idle: 你在說什麼啊？這裡就是莉亞的工作室。
    黑色利亞_idle: 我們這裡生產的作品都是莉亞原創作品。
    黑色利亞_win: 有本事就拿出證據啊？
    : （黑色利亞露出嘲諷的笑容）   
    主角: 當然！我們有證據！
    -> debate_start


// 用來紀錄辯論贏了幾場
VAR round_progress = 0


=== debate_start ===
MAI: 辯論環節開始！用最有利的線索來反駁他！
-> round_1

// --- 第一回合 ---
=== round_1 ===
黑色利亞_win: 哼，你說我盜圖？別開玩笑了！
黑色利亞_win: 看看這些作品，多漂亮又清楚，這種品質可不是隨便畫得出來的喔～

// 正確答案：線索2（作品細節模糊 - copy_machine）
~ start_debate_round("copy_machine")
-> DONE

// --- 第二回合 ---
=== round_2 ===
黑色利亞_idle2: 就算有些模糊，那也只是印刷問題啦！
黑色利亞_idle2: 我們每一幅作品都不一樣，都是我指導員工親手創作的！

// 正確答案：線索3（改作 - canvas）
~ start_debate_round("canvas")
-> DONE

// --- 第三回合 ---
=== round_3 ===
黑色利亞_idle3: 我...我們可沒偷誰的圖！
黑色利亞_idle3: 我們一切都靠自己完成，沒有從網路上抓圖，懂嗎！

// 正確答案：線索1（下載他人作品 - pc）
~ start_debate_round("pc")
-> DONE


// 辯論成功後

// --- 第一回合贏了（剛才解完影印機）---
=== debate_success_1 ===
MAI: 清楚？別騙人了！
MAI: 我仔細看過，那些畫的細節根本都糊成一團！
MAI: 真正的原作線條銳利、色彩飽滿，而你那些只是用機器複製出來的模糊假貨！

黑色利亞_lose: 可惡……那只是印刷問題啦！

: （黑色利亞少了一顆心，表情開始動搖）
// 進入第二回合
-> round_2


// --- 第二回合贏了（剛才解完畫布）---
=== debate_success_2 ===
MAI: 說謊！
MAI: 我們早就發現你那些『作品』全都是從同一張圖改出來的！
MAI: 工人只是照著複製圖，塗掉浮水印，再改幾筆就完事！
MAI: 這不是創作，是偽造！

黑色利亞_lose2: 你、你胡說八道！

// 進入第三回合
-> round_3

// --- 第三回合贏了（剛才解完電腦）---
=== debate_success_3 ===
MAI: 駁回！！！！
MAI: 哼，還想狡辯？
MAI: 你的電腦早就出賣你了！
MAI: 畫面上明明打開的是莉亞的作品頁面，工人正在不斷下載別人的作品！
MAI: 你偷、你印、你改，還敢說是自己做的？

黑色利亞_lose3: 不、不可能！那只是……

: （黑色利亞的身體開始逐漸消失...）

MAI: 就是現在！我們快點用相機師開始淨化！

MAI: 呼～剛才真是驚險！
MAI: 在回去之前，剛剛似乎撿到了一份卷軸，不介意的話我們一起來看看吧！

~ open_book("after_reading_plot")
-> DONE


// 失敗劇情
=== debate_failed ===
黑色利亞_win: 哈哈哈～這就是妳的『證據』？根本對不上嘛！
黑色利亞_win: 看來妳根本不懂藝術啊～
MAI: 不好！有奇怪的黑霧正在干擾我！
MAI: 快醒醒！妳的記憶正在被侵蝕！
MAI: 不──！意識要被吞掉了！
~ start_fail_vignette()
-> END


=== after_reading_plot ===
MAI: 時間不早了，我們快回去告訴莉亞我們的調查結果吧！
-> END



-> END
