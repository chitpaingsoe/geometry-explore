import React, { useState } from 'react';
import styled from 'styled-components';

import GoogleMapReact from 'google-map-react';
import { SGLOC } from './const/la_center';

const Wrapper = styled.main`
  width: 100%;
  height: 100%;
`;

const App = () => {
  const [marker, setMarker] = useState(null);
  const [map, setMap] = useState(null);
  const [maps, setMaps] = useState(null);
  const [bounds, setBounds] = useState(null);
  const [result, setResult] = useState([]);
  const [searching, setSearching] = useState(false);
  const [count, setCount] = useState(0);
  const [render, setRender] = useState(false);
  const [responseText, setResponseText] = useState(null);
  const [dataSource, setDataSource] = useState("/search");
  const [trackingTimer, setTrackingTimer] = useState(null);
  const [zoom, setZoom] = useState(12);





  // Fit map to its bounds after the api is loaded
  const apiIsLoaded = (map, maps) => {
    setMap(map);
    setMaps(maps);
    setBounds(map.getBounds().toJSON())
  };


  const updateBounds = (e) => {
    if (typeof (e) === "number") {
      setZoom(e);
    }
    if (map) {
      const coord = map.getBounds().toJSON();
      setBounds(coord);
      searchData(coord);
    }
  }

  const cleanup = () => {
    // setResponseText(null);
    setCount(0);
    if (marker) {
      marker.setMap(null);
      setMarker(null);
    }
    // if (result.length > 0) {
    //   result.map(m => {
    //     if (m.position) {
    //       m.setMap(null);
    //     }
    //   });
    //   setResult([]);
    // }
  }

  const searchData = (box) => {
    cleanup();
    if (!box) {
      alert("Cannot determine Bounding box!")
      return;
    }
    setSearching(true);
    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ south: box.south, west: box.west, north: box.north, east: box.east })
    };

    fetch('https://localhost:7063/api/bus' + dataSource, requestOptions)
      .then(response => response.json())
      .then(res => {
        let data = res.data;
        setResponseText(res.executionTime)
        if (render) {
          let result = data.map((val) => {
            return new maps.Marker({
              position: { lat: val.lat, lng: val.lng },
              map: map,
              title: 'Data'
            });
          })
          setResult(result)
        } else {
          setResult(data);
        }
        setSearching(false);
      });
  }

  const addDataInfo = (payload) => {

    const requestOptions = {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    };

    fetch('https://localhost:7063/api/bus', requestOptions)
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

      fetch('https://localhost:7063/api/bus', requestOptions)
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
    if (map) {
      const b = map.getBounds().toJSON();
      b.east -= 0.001;
      b.north += 0.001;
      map.fitBounds(b)
      map.setZoom(zoom);
    }
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
              <button onClick={_ => searchData(bounds)}>Search</button>&nbsp;&nbsp;
              <button onClick={cleanup}>Clear</button>&nbsp;&nbsp;
              <button onClick={deleteALlDataInfo}>Delete Data</button>
            </div>
          </div>

          <div style={{ marginTop: "20px" }}>
            {
              searching && (<div>Searching .............</div>)
            }
            {
              result.length > 0 ? (
                <div><b>ExectionTime: {responseText}ms, Result Count: {result.length}</b></div>
              ) : !searching && (<div>No data found in this area...</div>)
            }
          </div>
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
          >
          </GoogleMapReact>
        </Wrapper>
      </div>
    </div>
  );
};

export default App;
