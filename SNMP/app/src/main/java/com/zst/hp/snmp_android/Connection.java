package com.zst.hp.snmp_android;

import android.os.Bundle;

import android.util.Log;
import android.os.AsyncTask;
import android.widget.EditText;
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
import java.io.OutputStreamWriter;
import java.io.BufferedOutputStream;
import java.util.Arrays;
import java.io.PrintStream;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;



public class Connection extends AsyncTask<String, String, String> {

        PrintStream writer;
        InputStreamReader reader;
        BufferedReader br;
        String dstAddress;
        int dstPort;
        String response = "";
        EditText textResponse;
        String oid = "1.3.6.1.2.1.1.1.0";
        String recieve_message ="nic";

    private static Connection instance = null;
    protected Connection() {
        // Exists only to defeat instantiation.
    }
    public static Connection getInstance() {
        if(instance == null) {
            instance = new Connection();
        }
        return instance;
    }

    Connection(String addr, int port, EditText Response) {
        dstAddress = addr;
        dstPort = port;
        this.textResponse = Response;
        Log.i("BBBB","Connection");
        }

@Override
protected String doInBackground(String... params)
        {
            Socket socket = null;
            try
            {
                Log.i("BBB", "Proba polaczenia z" + dstAddress + " na " + dstPort);
                socket = new Socket(dstAddress, dstPort);
                Log.i("BBB", "Poloczono");
                writer = new PrintStream(socket.getOutputStream());

                br = new BufferedReader(new InputStreamReader(socket.getInputStream(),"UTF-8"));
                sendMessage(oid);
            while(true) {
                Log.i("BBB", "czekamy");
                System.out.println(br.readLine());
                //recieve_message =  br.readLine();
                Log.i("A",recieve_message);

               // Log.i("BBB", "odebrano: "+recieve_message);

                //writer.write(JSON);
                //OutputStream os = socket.getOutputStream();
                //ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream(
                //     1024);
                // byte[] buffer = new byte[1024];

                //BufferedReader br = new BufferedReader(new InputStreamReader(i));
                //DataOutputStream dos = new DataOutputStream(new BufferedOutputStream(socket.getOutputStream()));
                //buffer = s.getBytes();
                // byte[] bytes = in.getBytes();

                //PrintWriter pw = new PrintWriter(dos, true);
                //Log.i("AAA", "wysylam: ");
                //dos.writeBytes(s);
                //pw.println(in);

                //String wiadomosc = br.readLine();
                //Log.i("a", "czekam");
                //Log.i("A", wiadomosc);

            }
            } catch (UnknownHostException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            response = "UnknownHostException: " + e.toString();
            } catch (IOException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            response = "IOException: " + e.toString();
            }finally
            {
                if (socket != null)
                {
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


    public void sendMessage(String oid)
    {
        writer.println(oid);
        Log.i("BBB", "wysylanie");
    }
    public String getRecieveMessage()
    {
        return recieve_message;
    }
@Override
protected void onPostExecute(String result) {
        textResponse.setText(response);
        super.onPostExecute(result);
        }

}


