package com.dash.dancing.smash.game.tiles.circles.beat.piano.rhythm.hop;

import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.os.Build;
import android.app.Application;
import android.app.ActivityManager;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MainActivity extends UnityPlayerActivity {
    
    public static String getClipboardText(Context context) {
        ClipboardManager clipboard = (ClipboardManager) context.getSystemService(Context.CLIPBOARD_SERVICE);
        if (clipboard.hasPrimaryClip()) {
            ClipData clip = clipboard.getPrimaryClip();
            if (clip.getItemCount() > 0) {
                ClipData.Item item = clip.getItemAt(0);
                CharSequence clipboardText = item.getText();
                if (clipboardText!= null) {
                    return clipboardText.toString();
                }
            }
        }
        return null;
    }
}
