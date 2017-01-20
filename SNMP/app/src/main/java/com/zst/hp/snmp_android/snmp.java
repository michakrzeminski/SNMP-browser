package com.zst.hp.snmp_android;

import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.drawable.BitmapDrawable;
import android.support.v4.view.ViewPager;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.HashMap;
import java.util.List;
import java.util.Vector;

import android.app.Activity;
import android.os.Bundle;
import android.text.Layout;
import android.util.Log;
import android.view.GestureDetector;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ExpandableListView;
import android.widget.ExpandableListView.OnChildClickListener;
import android.widget.ExpandableListView.OnGroupClickListener;
import android.widget.ExpandableListView.OnGroupCollapseListener;
import android.widget.ExpandableListView.OnGroupExpandListener;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.TextView;
import android.widget.Toast;
import android.view.View;
import android.view.ViewGroup.LayoutParams;

public class snmp extends Activity {

    ListAdapter listAdapter;
    ExpandableListView expListView;
    List<String> listFolders;
    HashMap<String, List<String>> listChild;

    public static PopupWindow popUpWindow;
    private GestureDetector gestureDetector;

    static boolean connected = false;
    public static Connection connection;

    @Override
    protected void onCreate(Bundle savedInstanceState) {

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_snmp);
        // get the listview
        expListView = (ExpandableListView) findViewById(R.id.listview);

        // preparing list data
        prepareListData();

        listAdapter = new ListAdapter(this, listFolders, listChild);

        // setting list adapter
        expListView.setAdapter(listAdapter);

        gestureDetector = new GestureDetector(new SwipeGestureDetector());

        popUpWindow = new PopupWindow(this);

        // Listview on child click listener
        expListView.setOnChildClickListener(new OnChildClickListener() {

            @Override
            public boolean onChildClick(ExpandableListView parent, View v,int groupPosition, int childPosition, long id) {
                String name = listChild.get(listFolders.get(groupPosition)).get(childPosition);
                final String oid = getResources().getString(getResources().getIdentifier(name,"string",getPackageName()));

                //Popup
                LayoutInflater layoutInflater = (LayoutInflater)getBaseContext().getSystemService(LAYOUT_INFLATER_SERVICE);
                View popupView = layoutInflater.inflate(R.layout.popup, null);
                final PopupWindow popupWindow = new PopupWindow(popupView, LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
                popupWindow.setBackgroundDrawable(new BitmapDrawable());
                popupWindow.setOutsideTouchable(true);

                final TextView popupname = (TextView) popupWindow.getContentView().findViewById(R.id.PopupName);
                final TextView popupvalue = (TextView) popupWindow.getContentView().findViewById(R.id.PopupValue);
                Button btnOk = (Button)popupView.findViewById(R.id.ok);

                if(connected == true) {
                    connection.sendMessage(oid);
                }

                //wartosci tekstowe
                popupname.setText(name);

                try {
                    Thread.sleep(200);
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }

                if(connected == true)
                {
                    String temp = connection.getReceiveMessage();
                    popupvalue.setText(temp);
                }
                else {
                    Log.i("I","Not connected");
                    popupvalue.setText("Not connected");
                }

                btnOk.setOnClickListener(new Button.OnClickListener(){

                    @Override
                    public void onClick(View v) {
                        popupWindow.dismiss();
                    }});

                popupWindow.showAtLocation(findViewById(R.id.activity_snmp) , Gravity.CENTER, 0, 0);

                return false;
            }
        });

    }

    public static void connect(String ip, int port) {
        connection = new Connection(ip, port);
        connection.execute();
    }

    @Override
    public boolean onTouchEvent(MotionEvent event) {
        if (gestureDetector.onTouchEvent(event)) {
            return true;
        }
        return super.onTouchEvent(event);
    }

    private void onLeftSwipe() {
        Intent intent = new Intent(snmp.this, Settings.class);
        intent.addFlags(Intent.FLAG_ACTIVITY_REORDER_TO_FRONT);
        startActivity(intent);
    }

    private void prepareListData() {
        listFolders = new ArrayList<String>();
        listChild = new HashMap<String, List<String>>();

        // Adding child data
        listFolders.add("System");
        listFolders.add("Interfaces");
        listFolders.add("Ip");
        listFolders.add("Icmp");
        listFolders.add("Tcp");
        listFolders.add("Udp");
        listFolders.add("Snmp");
        listFolders.add("Host");

        List<String> system = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.System)));
        List<String> interfaces = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.Interfaces)));
        List<String> ip = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.Ip)));
        List<String> icmp = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.Icmp)));
        List<String> tcp = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.Tcp)));
        List<String> udp = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.Udp)));
        List<String> snmp = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.Snmp)));
        List<String> host = new ArrayList<String>(Arrays.asList(getResources().getStringArray(R.array.Host)));

        listChild.put(listFolders.get(0), system);
        listChild.put(listFolders.get(1), interfaces);
        listChild.put(listFolders.get(2), ip);
        listChild.put(listFolders.get(3), icmp);
        listChild.put(listFolders.get(4), tcp);
        listChild.put(listFolders.get(5), udp);
        listChild.put(listFolders.get(6), snmp);
        listChild.put(listFolders.get(7), host);

    }

    private class SwipeGestureDetector extends GestureDetector.SimpleOnGestureListener {
        // Swipe properties, you can change it to make the swipe
        // longer or shorter and speed
        private static final int SWIPE_MIN_DISTANCE = 60;
        private static final int SWIPE_MAX_OFF_PATH = 200;
        private static final int SWIPE_THRESHOLD_VELOCITY = 100;

        @Override
        public boolean onFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY) {
            try {
                float diffAbs = Math.abs(e1.getY() - e2.getY());
                float diff = e1.getX() - e2.getX();

                if (diffAbs > SWIPE_MAX_OFF_PATH)
                    return false;

                // Left swipe
                if (diff > SWIPE_MIN_DISTANCE
                        && Math.abs(velocityX) > SWIPE_THRESHOLD_VELOCITY) {
                    snmp.this.onLeftSwipe();
                }
            } catch (Exception e) {
                Log.e("Snmp", "Error on gestures");
            }
            return false;
        }
    }

    private static long back_pressed;

    @Override
    public void onBackPressed() {
        if (back_pressed + 2000 > System.currentTimeMillis())
        {
            super.onBackPressed();
        }
        else
            Toast.makeText(getBaseContext(), "Press once again to exit!", Toast.LENGTH_SHORT).show();
        back_pressed = System.currentTimeMillis();
    }

}
