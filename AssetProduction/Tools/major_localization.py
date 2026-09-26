import json
from pathlib import Path
translations={
'en':('Privacy','Your progress, settings and collection are stored on this device. This game has no account registration and does not send gameplay analytics to a server. You can delete your progress in Settings or clear the app data.','Read full policy'),
'ru':('Конфиденциальность','Прогресс, настройки и коллекция хранятся на этом устройстве. Игра не требует регистрации и не отправляет игровую аналитику на сервер. Удалить прогресс можно в настройках игры или очистив данные приложения.','Полная политика'),
'de':('Datenschutz','Fortschritt, Einstellungen und Sammlung werden auf diesem Gerät gespeichert. Das Spiel benötigt kein Konto und sendet keine Spielanalysen an einen Server. Du kannst deinen Fortschritt in den Einstellungen löschen oder die App-Daten entfernen.','Datenschutzerklärung'),
'tr':('Gizlilik','İlerlemen, ayarların ve koleksiyonun bu cihazda saklanır. Oyun hesap kaydı gerektirmez ve oyun analizlerini bir sunucuya göndermez. İlerlemeni Ayarlar üzerinden silebilir veya uygulama verilerini temizleyebilirsin.','Gizlilik politikasını oku'),
'ar':('الخصوصية','يتم حفظ تقدمك وإعداداتك ومجموعتك على هذا الجهاز. لا تتطلب اللعبة تسجيل حساب ولا ترسل تحليلات اللعب إلى خادم. يمكنك حذف تقدمك من الإعدادات أو مسح بيانات التطبيق.','قراءة السياسة الكاملة'),
'zh':('隐私','游戏进度、设置和收藏保存在此设备上。游戏无需注册账户，也不会向服务器发送游戏分析数据。你可以在设置中删除进度，或清除应用数据。','阅读完整政策')}
for code,values in translations.items():
    path=Path.cwd()/f'Assets/Resources/Localization/{code}.json'
    data=json.loads(path.read_text(encoding='utf-8-sig'))
    entries={e['key']:e for e in data['entries']}
    for key,value in zip(('UI_PRIVACY','UI_PRIVACY_BODY','UI_PRIVACY_OPEN'),values):
        if key in entries:entries[key]['value']=value
        else:data['entries'].append({'key':key,'value':value})
    path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('Major privacy copy added to six locales')
