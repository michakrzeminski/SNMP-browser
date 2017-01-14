package com.zst.hp.snmp_android;

import android.os.Bundle;

import android.util.Log;
import android.os.AsyncTask;
import android.widget.TextView;

import android.util.Log;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.DataOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.PrintWriter;
import java.net.Socket;
import java.io.ByteArrayOutputStream;
import java.net.UnknownHostException;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;



public class Connection extends AsyncTask<Void, Void, Void> {

        String dstAddress;
        int dstPort;
        String response = "";
        TextView textResponse;
        String in = "1.3.6.1.2.1.1.1";
        String s = "{\"Type\":\"SNMP_browser.SNMPQuery, SNMP-browser, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Value\":{\"oid\":\"1.3.6.1.2.1.1.1\"}}";

    Connection(String addr, int port, TextView textResponse) {
        dstAddress = addr;
        dstPort = port;
        this.textResponse = textResponse;
        Log.i("BBBB","Connection");

        }

@Override
protected Void doInBackground(Void... arg0)
        {

            Socket socket = null;


            try
            {
                Log.i("BBB","Proba polaczenia z" +dstAddress+" na "+ dstPort);
            socket = new Socket(dstAddress, dstPort);
                Log.i("BBB","Poloczono");
                Log.i("BBB","wysylanie");
                //writer = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream()));
                //writer.write(JSON);
                OutputStream os = socket.getOutputStream();
                PrintWriter pw = new PrintWriter(os, true);
                Log.i("AAA","wysylam"+s);
                pw.println(s);
                InputStream is = socket.getInputStream();
                BufferedReader br = new BufferedReader(
                        new InputStreamReader(is));
                Log.i("A",br.readLine());


            } catch (UnknownHostException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            response = "UnknownHostException: " + e.toString();
            } catch (IOException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            response = "IOException: " + e.toString();
            }finally {
            if (socket != null) {
            try {
            socket.close();
            } catch (IOException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            }
            }
            }
        return null;
        }

@Override
protected void onPostExecute(Void result) {
        textResponse.setText(response);
        super.onPostExecute(result);
        }

}


