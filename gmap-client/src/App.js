import React, { useState } from 'react';
import styled from 'styled-components';
import GoogleMapReact from 'google-map-react';
import { SGLOC } from './const/la_center';
import MarkerClusterer from "@google/markerclusterer";
import { v4 as uuid } from 'uuid';

const Wrapper = styled.main`
  width: 100%;
  height: 100%;
`;

const App = () => {
  const [markers, setMarkers] = useState([]);
  const [map, setMap] = useState(null);
  const [maps, setMaps] = useState(null);
  const [bounds, setBounds] = useState(null);
  const [searching, setSearching] = useState(false);
  const [count, setCount] = useState(0);
  const [render, setRender] = useState(false);
  const [responseText, setResponseText] = useState(null);
  const [dataSource, setDataSource] = useState("/search");
  const [trackingTimer, setTrackingTimer] = useState(null);
  const [zoom, setZoom] = useState(12);
  const [autorefresh, setAutoRefresh] = useState(false);
  const [autoRefreshTimer, setAutoRefreshTimer] = useState(null);
  const [markerclusters, setMarkerClusters] = useState([]);




  // Fit map to its bounds after the api is loaded
  const apiIsLoaded = (map, maps) => {
    setMap(map);
    setMaps(maps);
    setBounds(map.getBounds().toJSON());

  };


  const updateBounds = (e) => {
    if (typeof (e) === "number") {
      setZoom(e);
    }
    if (map) {
      const coord = map.getBounds().toJSON();
      setBounds(coord);
    }
  }

  const cleanup = () => {
    markerclusters.map(val => {
      let markercluster = val.cluster;
      if (markercluster !== null) {
        markercluster.clearMarkers();
        var tmp = markerclusters;
        var tmp1 = tmp.filter(x => x.id === val.id)
        setMarkerClusters([...tmp1])
      }
    })

  }

  const fetchData = () => {
    const box = map.getBounds().toJSON();
    cleanup();
    if (!box) {
      alert("Cannot determine Bounding box!")
      return;
    }
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ south: box.south, west: box.west, north: box.north, east: box.east })
    };

    return fetch('http://localhost:7063/api/bus' + dataSource, requestOptions);
  }

  const putData = (payload) => {
    const requestOptions = {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    };

    fetch('http://localhost:7063/api/bus', requestOptions)
      .then(response => response)
      .then(res => {
        console.log(res)
      });
  }

  const searchData = () => {
    cleanup();
    setSearching(true);
    fetchData()
      .then(response => response.json())
      .then(res => {
        let data = res.data;
        setResponseText(res.executionTime)
        setMarkers(data)
        let result = [];
        if (render) {
          data.map((i) => {
            result.push(new maps.Marker({
              position: { lat: i.lat, lng: i.lng },
              map: map,
            }));
          });
          setMarkers([...result]);
          let markerCluster = new MarkerClusterer(map, result, {
            imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m',
            gridSize: 10,
            minimumClusterSize: 2
          });
          let tmp = markerclusters;
          tmp.push({
            id: uuid(),
            cluster: markerCluster
          })
          setMarkerClusters([...tmp])
        } else {
          setMarkers(data);
        }
        setSearching(false);

        // update log
        var log = document.getElementById("log");
        if (log) {
          var date = new Date();
          console.log("append")
          log.innerHTML += `<p>At: ${date.getHours()}:${date.getMinutes()}:${date.getSeconds()}::${date.getMilliseconds()}, ExecutionTime: ${res.executionTime}ms, Count: ${data.length}</p>`
          log.scrollTop = log.scrollHeight;
        }
      });
  }

  const addDataInfo = (payload) => {

    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    };

    fetch('http://localhost:7063/api/bus', requestOptions)
      .then(response => response)
      .then(_ => {
        cleanup();
      });
  }

  const deleteALlDataInfo = () => {

    let confirm = prompt('Are you sure want to delet ALL DATA? [type: "yes" to confirm]');

    if (confirm === "yes") {
      const requestOptions = {
        method: 'DELETE',
        headers: { 'Content-Type': 'application/json' },
      };

      fetch('http://localhost:7063/api/bus', requestOptions)
        .then(response => response)
        .then(_ => {
          cleanup();
        });
    }

  }

  // define the function
  const getRandomPoint = () => {
    var x_min = bounds.east;
    var x_max = bounds.west;
    var y_min = bounds.south;
    var y_max = bounds.north;

    var lat = y_min + (Math.random() * (y_max - y_min));
    var lng = x_min + (Math.random() * (x_max - x_min));

    return { lat: lat, lng: lng }
  }

  const generatePoints = () => {
    if (!bounds) {
      alert("Cannot determine bounds!")
      return;
    }
    if (count < 1) {
      alert("Set Count!");
      return;
    }
    var requests = [];
    for (let i = 0; i < count; i++) {
      var point = getRandomPoint();
      var name = "GENData-" + i;
      requests.push({
        name: name,
        lat: point.lat,
        lng: point.lng
      })

    }
    addDataInfo(requests);
  }

  const simulateTracking = () => {
    fetchData()
      .then(response => response.json())
      .then(res => {
        let data = res.data;
        setResponseText(res.executionTime)
        let payload = data.map((i) => {
          return {
            id: i.id,
            name: i.name,
            lat: i.lat,
            lng: i.lng + 0.00005
          }
        });
        putData(payload);
      });
  }


  const startTracking = () => {
    var timer = setInterval(() => {
      simulateTracking();
    }, 1000);
    setTrackingTimer(timer);
  }

  const stopTracking = () => {
    if (trackingTimer) {
      clearInterval(trackingTimer);
      setTrackingTimer(null);
    }
  }

  return (
    <div style={{ height: '100vh', width: '100%' }}>
      <div style={{ height: '100vh', width: '20%', float: "left" }}>
        <div style={{ padding: "10px" }}>

          <div>
            <div>
              <input type="number" value={count} onChange={e => setCount(parseInt(e.target.value))} />&nbsp;&nbsp;
              <button onClick={generatePoints}>Generate Data</button>&nbsp;
              <span>(random point within box)</span>
            </div>
            <br />
            <br />
            <div>
              <label><input type="checkbox"
                value={render}
                defaultChecked={render}
                onChange={e => {
                  setRender(e.target.checked)
                }} /> &nbsp;View Markers</label>
            </div>
            <br />
            <div>
              <label><input type="checkbox"
                value={autorefresh}
                defaultChecked={autorefresh}
                onChange={e => {
                  setAutoRefresh(e.target.checked);
                  if (e.target.checked === true) {
                    var timer = setInterval(() => {
                      searchData();
                    }, 1500);
                    setAutoRefreshTimer(timer);
                  } else {
                    if (autoRefreshTimer) {
                      clearInterval(autoRefreshTimer);
                      setAutoRefreshTimer(null);
                    }
                  }
                }} /> &nbsp;AutoRefresh</label>
            </div>
            <br /> <br />
            <div>
              <button onClick={() => {
                if (trackingTimer) {
                  stopTracking();
                } else {
                  startTracking();
                }
              }}>{trackingTimer ? "Stop Tracking" : "Start Tracking"}</button> <br />
              <p>Query From:</p>
              <input type="radio" id="database" name="source" value="db" onChange={e => setDataSource("/search")} />
              <label htmlFor="database">Database</label><br />
              <input type="radio" id="cache" name="source" value="cache" onChange={e => setDataSource("/searchInCache")} />
              <label htmlFor="cache">Cache</label><br />

            </div>
            <br /> <br />
            <div>
              <button onClick={_ => searchData()}>Search</button>&nbsp;&nbsp;
              <button onClick={cleanup}>Clear</button>&nbsp;&nbsp;
              <button onClick={deleteALlDataInfo}>Delete Data</button>
            </div>
          </div>

          <div style={{ marginTop: "20px" }}>
            {
              searching ? (<div>Searching .............</div>) :

                markers.length > 0 ? (
                  <div><b>ExectionTime: {responseText}ms, Result Count: {markers.length}</b></div>
                ) : !searching && (<div>No data found in this area...</div>)
            }
          </div>
          <br />
          <div id="log" style={{
            color: "green",
            fontWeight: "bold",
            overflow: "auto",
            maxHeight: "400px"
          }}></div>
        </div>
      </div>
      <div style={{ height: '100vh', width: '80%', float: "right" }}>
        <Wrapper>
          <GoogleMapReact
            defaultZoom={zoom}
            defaultCenter={SGLOC}
            yesIWantToUseGoogleMapApiInternals
            onGoogleApiLoaded={({ map, maps }) => apiIsLoaded(map, maps)}
            onZoomAnimationEnd={updateBounds}
            onDragEnd={updateBounds}
            bootstrapURLKeys={{
              key: '',
              libraries: ['places', 'geometry', 'drawing', 'visualization']
            }}
          >
            {/* <MarkerClusterer
              options={{
                imagePath:
                  "https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m"
              }}
            >
              {markers.map((location, i) => (
                <Marker
                  key={i}
                  position={location}
                />
              ))
              }
            </MarkerClusterer> */}
          </GoogleMapReact>
        </Wrapper>
      </div>
    </div>
  );
};

export default App;
